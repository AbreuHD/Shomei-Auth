using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;


namespace Auth.Infraestructure.Identity.Features.Login.Queries.AuthLogin
{
    /// <summary>
    /// AuthLoginQuery Class, this class is used to login the user and generate a JWT Token for the user.
    /// </summary>
    /// <param LoginRequestDto="LoginRequestDto">
    /// <param UserAgent="UserAgent">
    /// <param IpAdress="IpAdress">
    /// <returns>
    /// <see cref="GenericApiResponse<AuthenticationResponse>"/>
    /// </returns>
    public class AuthLoginQuery : IRequest<GenericApiResponse<AuthenticationResponse>>
    {
        public required LoginRequestDto Dto { get; set; }
        public required string UserAgent { get; set; }
        public required string IpAdress { get; set; }
    }

    internal class AuthLoginQueryHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IdentityContext identityContext)
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
                UseDifferentProfiles = bool.Parse(Environment.GetEnvironmentVariable("UseDifferentProfiles") ?? configuration["JWTSettings:UseDifferentProfiles"] ?? "0"),
                DurationInMinutes = int.Parse(Environment.GetEnvironmentVariable("DurationInMinutes") ?? configuration["JWTSettings:DurationInMinutes"] ?? "0")
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
                var result = await _signInManager.PasswordSignInAsync(User.UserName!, request.Dto.Password, false, lockoutOnFailure: false);
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
                        Token = ExtraMethods.HashToken(token),
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
