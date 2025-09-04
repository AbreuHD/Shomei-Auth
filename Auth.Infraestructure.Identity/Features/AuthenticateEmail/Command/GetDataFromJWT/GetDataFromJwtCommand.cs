using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth.Infraestructure.Identity.Features.AuthenticateEmail.Command.GetDataFromJWT
{
    /// <summary>
    /// Command to retrieve the current authenticated user's data (from JWT claims)
    /// and return Identity info by looking up the user via Id.
    /// </summary>
    public class GetDataFromJwtCommand : IRequest<GenericApiResponse<UserDataDto>>
    {
        /// <summary>
        /// If true, includes roles in the response (extra DB call).
        /// </summary>
        public bool IncludeRoles { get; set; } = true;
    }

    public class UserDataDto
    {
        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public IEnumerable<string>? Roles { get; set; }
    }

    internal class GetDataFromJwtCommandHandler(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager) : IRequestHandler<GetDataFromJwtCommand, GenericApiResponse<UserDataDto>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<GenericApiResponse<UserDataDto>> Handle(GetDataFromJwtCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<UserDataDto>
            {
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = "N/A"
            };

            try
            {
                var principal = _httpContextAccessor.HttpContext?.User;

                if (principal == null || !principal.Identity?.IsAuthenticated == true)
                {
                    response.Success = false;
                    response.Statuscode = StatusCodes.Status401Unauthorized;
                    response.Message = "Unauthorized: no authenticated user found.";
                    return response;
                }

                var userId = principal.FindFirstValue("uid");

                if (string.IsNullOrWhiteSpace(userId))
                {
                    response.Success = false;
                    response.Statuscode = StatusCodes.Status400BadRequest;
                    response.Message = "No userId claim found in token.";
                    return response;
                }

                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                {
                    response.Success = false;
                    response.Statuscode = StatusCodes.Status404NotFound;
                    response.Message = $"User with Id '{userId}' not found.";
                    return response;
                }

                var dto = new UserDataDto
                {
                    Id = appUser.Id,
                    UserName = appUser.UserName!,
                    Email = appUser.Email,
                    EmailConfirmed = appUser.EmailConfirmed,
                    PhoneNumber = appUser.PhoneNumber,
                    PhoneNumberConfirmed = appUser.PhoneNumberConfirmed
                };

                if (request.IncludeRoles)
                {
                    var roles = await _userManager.GetRolesAsync(appUser);
                    dto.Roles = roles;
                }

                response.Success = true;
                response.Statuscode = StatusCodes.Status200OK;
                response.Message = "OK";
                response.Payload = dto;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Statuscode = StatusCodes.Status500InternalServerError;
                response.Message = $"An error occurred while retrieving the user data. {ex.Message}";
                response.Payload = default!;
                return response;
            }
        }
    }
}
