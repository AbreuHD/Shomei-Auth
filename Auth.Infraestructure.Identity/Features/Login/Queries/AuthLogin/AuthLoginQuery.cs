using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;


namespace Auth.Infraestructure.Identity.Features.Login.Queries.AuthLogin
{
    /// <summary>
    /// AuthLoginQuery Class, this class is used to login the user and generate a JWT Token for the user.
    /// </summary>
    /// <param UserName="UserName">
    /// <param Password="Password">
    /// <returns>
    /// <see cref="GenericApiResponse<AuthenticationResponse>"/>
    /// </returns>
    public class AuthLoginQuery : IRequest<GenericApiResponse<AuthenticationResponse>>
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }

    public class AuthLoginQueryHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        : IRequestHandler<AuthLoginQuery, GenericApiResponse<AuthenticationResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly JWTSettings _jwtSettings
            = new()
            {
                Audience = Environment.GetEnvironmentVariable("Audience") ?? configuration["JWTSettings:Audience"] ?? string.Empty,
                Issuer = Environment.GetEnvironmentVariable("Issuer") ?? configuration["JWTSettings:Issuer"] ?? string.Empty,
                Key = Environment.GetEnvironmentVariable("Key") ?? configuration["JWTSettings:Key"] ?? string.Empty,
                DurationInMinutes = int.Parse(Environment.GetEnvironmentVariable("DurationInMinutes") ?? configuration["JWTSettings:DurationInMinutes"] ?? "0")
            };

        public async Task<GenericApiResponse<AuthenticationResponse>> Handle(AuthLoginQuery request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<AuthenticationResponse>();
            var User = await _userManager.FindByNameAsync(request.UserName);
            if (User == null)
            {
                response.Success = false;
                response.Message = $"No Account Register with {request.UserName}";
                response.Statuscode = 402;
                return response;
            }
            var result = await _signInManager.PasswordSignInAsync(User.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                response.Success = false;
                response.Message = $"Invalid Password";
                response.Statuscode = 409;
                return response;
            }
            if (!User.EmailConfirmed)
            {
                response.Success = false;
                response.Message = $"Account not confirm for {request.UserName}";
                response.Statuscode = 400;
                return response;
            }

            var userClaims = await _userManager.GetClaimsAsync(User);
            var roles = await _userManager.GetRolesAsync(User);

            JwtSecurityToken jwtSecurityToken = ExtraMethods.GenerateJWToken(User, _jwtSettings, userClaims, roles, null);
            response.Payload = new AuthenticationResponse
            {
                Id = User.Id,
                Name = User.Name,
                LastName = User.LastName,
                Email = User.Email,
                IsVerified = User.EmailConfirmed,
                Roles = [.. roles],
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                RefreshToken = ExtraMethods.GenerateRefreshToken().Token
            };

            return response;
        }


    }
}
