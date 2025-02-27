using Auth.Infraestructure.Identity.DTOs.Email;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    public class ChangeEmailCommand : IRequest<GenericApiResponse<bool>>
    {
        public required ChangeEmailRequestDto Dto { get; set; }
        public required string UserId { get; set; }
    }
    internal class ChangeEmailCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ChangeEmailCommand, GenericApiResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<GenericApiResponse<bool>> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
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
                var UserNameExist = await _userManager.FindByEmailAsync(request.Dto.NewEmail);
                if (UserNameExist != null)
                {
                    response.Success = false;
                    response.Message = $"Email {request.Dto.NewEmail} is already taken";
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
                var result = await _userManager.SetEmailAsync(user, request.Dto.NewEmail);
                if (result.Succeeded)
                {
                    response.Payload = true;
                    response.Success = true;
                    response.Statuscode = StatusCodes.Status200OK;
                    response.Message = "Email changed successfully.";
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
