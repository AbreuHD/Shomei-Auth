using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Mail;
using Auth.Infraestructure.Identity.DTOs.Password;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Mails;
using Auth.Infraestructure.Identity.Otp;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.ForgotPSW.Commands
{
    public class ChangePasswordWithOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        public required ChangePasswordWithOtpRequestDto Dto { get; set; }
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
    }
    internal class ChangePasswordWithOtpCommandHandler(UserManager<ApplicationUser> userManager,
            MailSettings mailSettings,
            IHttpClientFactory httpClientFactory,
            IdentityContext identityContext) : IRequestHandler<ChangePasswordWithOtpCommand, GenericApiResponse<bool>>
    {
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> Handle(ChangePasswordWithOtpCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<bool>()
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };

            var user = await _userManager.FindByEmailAsync(request.Dto.Email);
            if (user == null)
            {
                response.Message = "User with that Email not found";
                response.Statuscode = StatusCodes.Status404NotFound;
                return response;
            }

            var otpCheckResponse = await ValidateOtpEmail.ValidateOtpWithEmail(response, request.Dto.Otp, user.Id, OtpPurpose.PasswordReset.ToString(), _identityContext);
            if (!otpCheckResponse.Success)
            {
                return otpCheckResponse;
            }

            if (request.Dto.NewPassword != request.Dto.RepeatNewPassword)
            {
                response.Message = "Passwords do not match";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            if (await _userManager.CheckPasswordAsync(user, request.Dto.NewPassword))
            {
                response.Message = "New password cannot be the same as the current password";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }
            var passwordReset = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, passwordReset, request.Dto.NewPassword);

            if (result.Succeeded)
            {
                response.Payload = true;
                response.Success = true;
                response.Statuscode = StatusCodes.Status200OK;
                response.Message = "Password changed successfully";

                if (request.UserAgent != null && request.IpAddress != null)
                {
                    var geoInfo = await ExtraMethods.GetGeoLocationInfo(request.IpAddress, _httpClientFactory);
                    await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                    {
                        To = user.Email!,
                        Body = ChangePasswordMail.GetEmailHtml(request.IpAddress, geoInfo?.Country ?? "", request.UserAgent),
                        Subject = "Password Changed"
                    });
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    response.Message += " " + error.Description;
                }
                response.Statuscode = StatusCodes.Status406NotAcceptable;
            }

            return response;
        }
    }
}
