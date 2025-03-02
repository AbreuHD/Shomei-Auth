using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Email;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Otp;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    public class ChangeEmailWithOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        public required ChangeEmailWithOtpRequestDto Dto { get; set; }
        public required string UserId { get; set; }
    }
    internal class ChangeEmailWithOtpCommandHandler(UserManager<ApplicationUser> userManager,
            IdentityContext identityContext) : IRequestHandler<ChangeEmailWithOtpCommand, GenericApiResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> Handle(ChangeEmailWithOtpCommand request, CancellationToken cancellationToken)
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

                var otpCheckResponse = await ValidateOtpEmail.ValidateOtpWithEmail(response, request.Dto.Otp, user.Id, OtpPurpose.ChangeEmail.ToString(), _identityContext);
                if (!otpCheckResponse.Success)
                {
                    return otpCheckResponse;
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