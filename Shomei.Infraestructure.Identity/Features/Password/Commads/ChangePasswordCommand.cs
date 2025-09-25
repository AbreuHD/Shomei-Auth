using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.DTOs.Mail;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Extra;
using Shomei.Infraestructure.Identity.Mails;
using Shomei.Infraestructure.Identity.Settings;

namespace Shomei.Infraestructure.Identity.Features.Password.Commads
{
    /// <summary>
    /// Represents a command to change a user's password.
    /// This command is processed by the corresponding handler.
    /// </summary>
    public class ChangePasswordCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// Gets or sets the current password of the user.
        /// This is required to verify the user's identity before changing the password.
        /// </summary>
        public required string CurrentPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password that the user wants to set.
        /// Must meet the system's password security requirements.
        /// </summary>
        public required string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the repeated entry of the new password.
        /// Used to confirm that the user has entered the intended new password correctly.
        /// </summary>
        public required string RepeatNewPassword { get; set; }
    }
    internal class ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager,
            MailSettings mailSettings,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor
        ) : IRequestHandler<ChangePasswordCommand, GenericApiResponse<bool>>
    {
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var IpAdress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";
            var UserId = _httpContextAccessor.HttpContext?.User.FindFirst("uid")?.Value ?? "Unknown";
            var response = new GenericApiResponse<bool>()
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                response.Message = "User not found";
                response.Statuscode = StatusCodes.Status404NotFound;
                return response;
            }

            if (request.NewPassword != request.RepeatNewPassword)
            {
                response.Message = "Passwords do not match";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordCheck)
            {
                response.Message = "Current password is incorrect";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            if (request.NewPassword == request.CurrentPassword)
            {
                response.Message = "New password cannot be the same as the current password";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                response.Payload = true;
                response.Success = true;
                response.Statuscode = StatusCodes.Status200OK;
                response.Message = "Password changed successfully";

                if (UserAgent != null && IpAdress != null)
                {
                    var geoInfo = await ExtraMethods.GetGeoLocationInfo(IpAdress, _httpClientFactory);
                    await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                    {
                        To = user.Email!,
                        Body = ChangePasswordMail.GetEmailHtml(IpAdress, geoInfo?.Country ?? "", UserAgent),
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
