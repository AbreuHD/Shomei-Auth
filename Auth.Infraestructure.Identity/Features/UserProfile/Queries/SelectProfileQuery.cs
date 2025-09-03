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

namespace Auth.Infraestructure.Identity.Features.UserProfile.Queries
{
    /// <summary>
    /// Query for selecting a user profile and generating a JWT token for the selected profile.
    /// </summary>
    /// <remarks>
    /// This query is used to fetch the user profile, validate the profile's association with the user,
    /// and generate a JWT token that includes the user's profile information.
    /// </remarks>
    public class SelectProfileQuery : IRequest<GenericApiResponse<AuthenticationResponse>>
    {
        /// <summary>
        /// The data transfer object containing the profile selection information.
        /// </summary>
        /// <remarks>
        /// This object holds the profile ID selected by the user for authentication and token generation.
        /// </remarks>
        public required SelectProfileRequestDto Dto { get; set; }

        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        /// <remarks>
        /// This is used to find the correct user profile and authenticate the user.
        /// </remarks>
        public required string UserId { get; set; }

        /// <summary>
        /// The user agent string from the device making the request.
        /// </summary>
        /// <remarks>
        /// This is used for logging and tracking the user's session details.
        /// </remarks>
        public required string UserAgent { get; set; }

        /// <summary>
        /// The IP address of the device making the request.
        /// </summary>
        /// <remarks>
        /// This is used for security purposes and logging the user's session.
        /// </remarks>
        public required string IpAdress { get; set; }
    }
    internal class SelectProfileQueryHandler(UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IdentityContext identityContext) : IRequestHandler<SelectProfileQuery, GenericApiResponse<AuthenticationResponse>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly JwtSettings _jwtSettings
            = new()
            {
                Audience = Environment.GetEnvironmentVariable("Audience") ?? configuration["JWTSettings:Audience"] ?? string.Empty,
                Issuer = Environment.GetEnvironmentVariable("Issuer") ?? configuration["JWTSettings:Issuer"] ?? string.Empty,
                UseDifferentProfiles = bool.Parse(Environment.GetEnvironmentVariable("UseDifferentProfiles") ?? configuration["JWTSettings:UseDifferentProfiles"] ?? "false"),
                Key = Environment.GetEnvironmentVariable("Key") ?? configuration["JWTSettings:Key"] ?? string.Empty,
                DurationInMinutes = int.Parse(Environment.GetEnvironmentVariable("DurationInMinutes") ?? configuration["JWTSettings:DurationInMinutes"] ?? "0")
            };

        public async Task<GenericApiResponse<AuthenticationResponse>> Handle(SelectProfileQuery request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<AuthenticationResponse>()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "N/A"
            };
            try
            {
                var profileResponse = await _identityContext.Set<Entities.UserProfile>().FindAsync([request.Dto.Profile], cancellationToken: cancellationToken);
                if (profileResponse is null)
                {
                    return new GenericApiResponse<AuthenticationResponse>
                    {
                        Success = false,
                        Message = $"No Profile Register with {request.Dto.Profile}",
                        Statuscode = StatusCodes.Status500InternalServerError
                    };
                }
                if (profileResponse.UserId != request.UserId)
                {
                    return new GenericApiResponse<AuthenticationResponse>
                    {
                        Success = false,
                        Message = $"Profile {request.Dto.Profile} does not belong to {request.UserId}",
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

                JwtSecurityToken jwtSecurityToken = ExtraMethods.GenerateJWToken(userResponse, _jwtSettings, userClaims, roles, request.Dto.Profile);
                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                response.Payload = new AuthenticationResponse
                {
                    Id = userResponse.Id,
                    Name = userResponse.Name,
                    LastName = userResponse.LastName,
                    UserName = userResponse.UserName!,
                    Email = userResponse.Email!,
                    IsVerified = userResponse.EmailConfirmed,
                    Roles = [.. roles],
                    JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    RefreshToken = ExtraMethods.GenerateRefreshToken().Token
                };

                var session = new UserSession
                {
                    UserId = userResponse.Id,
                    Token = ExtraMethods.HashToken(token),
                    Expiration = jwtSecurityToken.ValidTo,
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = request.IpAdress,
                    UserAgent = request.UserAgent
                };
                _identityContext.Set<UserSession>().Add(session);
                await _identityContext.SaveChangesAsync(cancellationToken);
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
