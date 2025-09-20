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
        /// The ID of the profile to be selected.
        /// </summary>
        public required int ProfileId { get; set; }
    }
    internal class SelectProfileQueryHandler(UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IdentityContext identityContext,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<SelectProfileQuery, GenericApiResponse<AuthenticationResponse>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
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
            var user = _httpContextAccessor.HttpContext?.User;
            var uidClaim = user?.FindFirst("uid")?.Value;
            if (uidClaim == null)
            {
                return new GenericApiResponse<AuthenticationResponse> { Success = false, Message = "You are not logged", Statuscode = StatusCodes.Status400BadRequest };
            }
            var UserAgent = _httpContextAccessor.HttpContext!.Request.Headers.UserAgent.ToString() ?? "Unknown";
            var IpAdress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var response = new GenericApiResponse<AuthenticationResponse>()
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "N/A"
            };
            try
            {
                var profileResponse = await _identityContext.Set<Entities.UserProfile>().FindAsync([request.ProfileId], cancellationToken: cancellationToken);
                if (profileResponse is null)
                {
                    return new GenericApiResponse<AuthenticationResponse>
                    {
                        Success = false,
                        Message = $"No Profile Register with {request.ProfileId}",
                        Statuscode = StatusCodes.Status500InternalServerError
                    };
                }
                if (profileResponse.UserId != uidClaim)
                {
                    return new GenericApiResponse<AuthenticationResponse>
                    {
                        Success = false,
                        Message = $"Profile {request.ProfileId} does not belong to {uidClaim}",
                        Statuscode = StatusCodes.Status401Unauthorized
                    };
                }
                var userResponse = await _userManager.FindByIdAsync(uidClaim);
                if (userResponse is null)
                {
                    return new GenericApiResponse<AuthenticationResponse>
                    {
                        Success = false,
                        Message = $"No Account Register with {uidClaim}",
                        Statuscode = StatusCodes.Status500InternalServerError,
                    };
                }
                var userClaims = await _userManager.GetClaimsAsync(userResponse);
                var roles = await _userManager.GetRolesAsync(userResponse);

                JwtSecurityToken jwtSecurityToken = ExtraMethods.GenerateJWToken(userResponse, _jwtSettings, userClaims, roles, request.ProfileId);
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
                    Token = ExtraMethods.GetHash(token),
                    Expiration = jwtSecurityToken.ValidTo,
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = IpAdress,
                    UserAgent = UserAgent
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
