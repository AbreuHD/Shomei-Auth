using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Mail;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Mails;
using Auth.Infraestructure.Identity.Migrations;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;


namespace Auth.Infraestructure.Identity.Features.Login.Queries.AuthLogin
{
    /// <summary>
    /// Represents a query used to authenticate a user and generate a JWT Token for login.
    /// It validates the user credentials and returns the authentication details, including the JWT token and refresh token.
    /// </summary>
    public class AuthLoginQuery : IRequest<GenericApiResponse<AuthenticationResponse>>
    {
        /// <summary>
        /// The data transfer object (DTO) containing the username and password for user authentication.
        /// </summary>
        /// <value>
        /// A <see cref="LoginRequestDto"/> that holds the username and password for authentication.
        /// </value>
        public required LoginRequestDto Dto { get; set; }

        /// <summary>
        /// The user agent string representing the client's browser or application information.
        /// </summary>
        /// <value>
        /// A string representing the user agent.
        /// </value>
        public required string UserAgent { get; set; }

        /// <summary>
        /// The IP address of the client making the login request.
        /// </summary>
        /// <value>
        /// A string representing the IP address.
        /// </value>
        public required string IpAdress { get; set; }
    }

    internal class AuthLoginQueryHandler(UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager, 
        IConfiguration configuration, 
        IdentityContext identityContext,
        IHttpClientFactory _httpClientFactory,
        MailSettings _mailSettings)

        : IRequestHandler<AuthLoginQuery, GenericApiResponse<AuthenticationResponse>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        private readonly JwtSettings _jwtSettings
            = new()
            {
                Audience = Environment.GetEnvironmentVariable("Audience") ?? configuration["JWTSettings:Audience"] ?? string.Empty,
                Issuer = Environment.GetEnvironmentVariable("Issuer") ?? configuration["JWTSettings:Issuer"] ?? string.Empty,
                Key = Environment.GetEnvironmentVariable("Key") ?? configuration["JWTSettings:Key"] ?? string.Empty,
                UseDifferentProfiles = bool.Parse(Environment.GetEnvironmentVariable("UseDifferentProfiles") ?? configuration["JWTSettings:UseDifferentProfiles"] ?? "false"),
                DurationInMinutes = int.Parse(Environment.GetEnvironmentVariable("DurationInMinutes") ?? configuration["JWTSettings:DurationInMinutes"] ?? "0"),
                MaxFailedAccessAttempts = int.Parse(Environment.GetEnvironmentVariable("MaxFailedAccessAttempts") ?? configuration["JWTSettings:MaxFailedAccessAttempts"] ?? "10"),
                DefaultLockoutTimeSpan = int.Parse(Environment.GetEnvironmentVariable("DefaultLockoutTimeSpan") ?? configuration["JWTSettings:DefaultLockoutTimeSpan"] ?? "30")
            };

        public async Task<GenericApiResponse<AuthenticationResponse>> Handle(AuthLoginQuery request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<AuthenticationResponse>()
            {
                Message = "Logged",
                Statuscode = StatusCodes.Status200OK,
                Success = true,
                Payload = new AuthenticationResponse()
                {
                    Id = string.Empty,
                    Name = string.Empty,
                    LastName = string.Empty,
                    UserName = string.Empty,
                    Email = string.Empty,
                    Roles = [],
                    IsVerified = false,
                    JWToken = string.Empty,
                    RefreshToken = string.Empty,
                }
            };

            try
            {
                var User = await _userManager.FindByNameAsync(request.Dto.UserName);
                if (User == null)
                {
                    response.Success = false;
                    response.Message = $"No Account Register with {request.Dto.UserName}";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    return response;
                }
                bool isFinalAttempt = User.AccessFailedCount == (_jwtSettings.MaxFailedAccessAttempts - 1);

                var result = await _signInManager.PasswordSignInAsync(User.UserName!, request.Dto.Password, false, lockoutOnFailure: true);

                if (User.IsBanned == true)
                {
                    response.Success = false;
                    response.Message = $"Account Banned unitl {User.LockoutEnd}";
                    response.Statuscode = StatusCodes.Status401Unauthorized;
                    return response;
                }

                if (result.IsLockedOut && isFinalAttempt)
                {
                    var blockEmail = new AccountLockEmail()
                    {
                        UserName = User.UserName!,
                        Ip = request.IpAdress,
                        Country = (await ExtraMethods.GetGeoLocationInfo(request.IpAdress, _httpClientFactory))?.Country ?? "Unknown",
                        UserAgent = request.UserAgent,
                        LockDuration = $"{_jwtSettings.DefaultLockoutTimeSpan} minutes"
                    };

                    await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                    {
                        To = User.Email!,
                        Body = blockEmail.GetEmailHtml(),
                        Subject = "Confirm your email"
                    });

                    response.Success = false;
                    response.Message = $"Account has been locked for multiple failed attempts. Please try again in {_jwtSettings.DefaultLockoutTimeSpan} minutes.";
                    response.Statuscode = StatusCodes.Status423Locked;
                    return response;
                }
                if (result.IsLockedOut)
                {
                    response.Success = false;
                    response.Message = $"Account Locked/Banned until {User.LockoutEnd}";
                    response.Statuscode = StatusCodes.Status423Locked;
                    return response;
                }
                if (result.RequiresTwoFactor)
                {
                    response.Success = false;
                    response.Message = $"Two Factor Authentication Required";
                    response.Statuscode = StatusCodes.Status401Unauthorized;
                    return response;
                }
                if (!result.Succeeded)
                {
                    response.Success = false;
                    response.Message = $"Invalid Password";
                    response.Statuscode = StatusCodes.Status409Conflict;
                    return response;
                }
                if (!User.EmailConfirmed)
                {
                    response.Success = false;
                    response.Message = $"Account not confirm for {request.Dto.UserName}";
                    response.Statuscode = StatusCodes.Status400BadRequest;
                    return response;
                }

                var userClaims = await _userManager.GetClaimsAsync(User);
                var roles = await _userManager.GetRolesAsync(User);

                JwtSecurityToken jwtSecurityToken = ExtraMethods.GenerateJWToken(User, _jwtSettings, userClaims, roles, null);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                if (!_jwtSettings.UseDifferentProfiles)
                {
                    var session = new UserSession
                    {
                        UserId = User.Id,
                        Token = ExtraMethods.GetHash(token),
                        Expiration = jwtSecurityToken.ValidTo,
                        IpAddress = request.IpAdress,
                        UserAgent = request.UserAgent,
                        CreatedAt = DateTime.UtcNow
                    };

                    _identityContext.Set<UserSession>().Add(session);
                    await _identityContext.SaveChangesAsync(cancellationToken);
                }

                response.Payload = new AuthenticationResponse
                {
                    Id = User.Id,
                    Name = User.Name,
                    UserName = User.UserName!,
                    LastName = User.LastName,
                    Email = User.Email!,
                    IsVerified = User.EmailConfirmed,
                    Roles = [.. roles],
                    JWToken = token,
                    RefreshToken = ExtraMethods.GenerateRefreshToken().Token
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Statuscode = StatusCodes.Status500InternalServerError;
            }
            return response;
        }
    }
}
