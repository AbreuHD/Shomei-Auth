using Auth.Core.Application.DTOs.Account;
using Auth.Core.Application.DTOs.Generic;
using Auth.Core.Application.Enums;
using Auth.Core.Application.Settings;
using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Queries
{
    /// <summary>
    /// SelectProfileQuery Class, this class is used to get the user and generate a JWT Token for the user with his profileId selected.
    /// </summary>
    /// <param UserName="UserName">
    /// <param Password="Password">
    /// <returns>
    /// <see cref="GenericApiResponse<AuthenticationResponse>"/>
    /// </returns>
    public class SelectProfileQuery : IRequest<GenericApiResponse<AuthenticationResponse>>
    {
        [JsonIgnore]
        public string? UserId { get; set; }
        public int? Profile { get; set; }
    }
    public class SelectProfileQueryHandler(UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IdentityContext identityContext) : IRequestHandler<SelectProfileQuery, GenericApiResponse<AuthenticationResponse>>
        {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly JWTSettings _jwtSettings
            = new()
            {
                Audience = Environment.GetEnvironmentVariable("Audience") ?? configuration["JWTSettings:Audience"] ?? string.Empty,
                Issuer = Environment.GetEnvironmentVariable("Issuer") ?? configuration["JWTSettings:Issuer"] ?? string.Empty,
                Key = Environment.GetEnvironmentVariable("Key") ?? configuration["JWTSettings:Key"] ?? string.Empty,
                DurationInMinutes = int.Parse(Environment.GetEnvironmentVariable("DurationInMinutes") ?? configuration["JWTSettings:DurationInMinutes"] ?? "0")
            };

        public async Task<GenericApiResponse<AuthenticationResponse>> Handle(SelectProfileQuery request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<AuthenticationResponse>();
            var profileResponse = await _identityContext.Set<Entities.UserProfile>().FindAsync(request.Profile);
            if (profileResponse is null)
            {
                return new GenericApiResponse<AuthenticationResponse>
                {
                    Success = false,
                    Message = $"No Profile Register with {request.Profile}",
                    Statuscode = StatusCodes.Status500InternalServerError
                };
            }
            if(profileResponse.UserId != request.UserId)
            {
                return new GenericApiResponse<AuthenticationResponse>
                {
                    Success = false,
                    Message = $"Profile {request.Profile} does not belong to {request.UserId}",
                    Statuscode = StatusCodes.Status401Unauthorized
                };
            }
            var userResponse = await _userManager.FindByIdAsync(request.UserId ?? String.Empty);
            if (userResponse is null)
            {
                return new GenericApiResponse<AuthenticationResponse>
                {
                    Success = false,
                    Message = $"No Account Register with {request.UserId}",
                    Statuscode = StatusCodes.Status500InternalServerError,
                };
            }
            var userClaims = await _userManager.GetClaimsAsync(userResponse);
            var roles = await _userManager.GetRolesAsync(userResponse);

            JwtSecurityToken jwtSecurityToken = ExtraMethods.GenerateJWToken(userResponse, _jwtSettings, userClaims, roles, request.Profile);
            response.Payload = new AuthenticationResponse
            {
                Id = userResponse.Id,
                Name = userResponse.Name,
                LastName = userResponse.LastName,
                Email = userResponse.Email,
                IsVerified = userResponse.EmailConfirmed,
                Roles = [.. roles],
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                RefreshToken = ExtraMethods.GenerateRefreshToken().Token
            };

            return response;

        }
    }

}
