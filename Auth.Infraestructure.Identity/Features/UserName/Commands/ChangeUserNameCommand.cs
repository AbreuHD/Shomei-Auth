using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.UserName;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.Features.UserName.Commands
{
    /// <summary>
    /// Represents a command to change a user's username.
    /// This command is processed by the corresponding handler.
    /// </summary>
    public class ChangeUserNameCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user requesting the username change.
        /// This ID is used to retrieve the user from the identity system.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Gets or sets the data transfer object (DTO) containing the new username details.
        /// </summary>
        public required ChangeUserNameRequestDto Dto { get; set; }
    }
    internal class ChangeUserNameCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ChangeUserNameCommand, GenericApiResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<GenericApiResponse<bool>> Handle(ChangeUserNameCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<bool>()
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    response.Message = "User not found.";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    return response;
                }
                var UserNameExist = await _userManager.FindByNameAsync(request.Dto.NewUserName);
                if (UserNameExist != null)
                {
                    response.Success = false;
                    response.Message = $"Username {request.Dto.NewUserName} is already taken";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    return response;
                }
                var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Dto.Password);
                if (!passwordCheck)
                {
                    response.Message = "Password is incorrect";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    return response;
                }
                var result = await _userManager.SetUserNameAsync(user, request.Dto.NewUserName);
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
