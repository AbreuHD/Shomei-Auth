using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Entities;

namespace Shomei.Infraestructure.Identity.Features.UserName.Commands
{
    /// <summary>
    /// Represents a command to change a user's username.
    /// This command is processed by the corresponding handler.
    /// </summary>
    public class ChangeUserNameCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// Gets or sets the current password of the user.
        /// This is required to verify the user's identity before allowing the username change.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Gets or sets the new username that the user wants to set.
        /// Must meet the system's username validation requirements.
        /// </summary>
        public required string NewUserName { get; set; }
    }
    internal class ChangeUserNameCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor) : IRequestHandler<ChangeUserNameCommand, GenericApiResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<GenericApiResponse<bool>> Handle(ChangeUserNameCommand request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext?.User.FindFirst("uid")?.Value ?? "Unknown";
            var response = new GenericApiResponse<bool>()
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };
            try
            {
                var user = await _userManager.FindByIdAsync(UserId);
                if (user == null)
                {
                    response.Message = "User not found.";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    return response;
                }
                var UserNameExist = await _userManager.FindByNameAsync(request.NewUserName);
                if (UserNameExist != null)
                {
                    response.Success = false;
                    response.Message = $"Username {request.NewUserName} is already taken";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    return response;
                }
                var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordCheck)
                {
                    response.Message = "Password is incorrect";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    return response;
                }
                var result = await _userManager.SetUserNameAsync(user, request.NewUserName);
                if (result.Succeeded)
                {
                    response.Payload = true;
                    response.Success = true;
                    response.Statuscode = StatusCodes.Status200OK;
                    response.Message = "Username changed successfully.";
                }
                else
                {
                    response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
