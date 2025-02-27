using Auth.Infraestructure.Identity.DTOs.Mail;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Password;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Mails;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Password.Commads
{
    /// <summary>
    /// Represents a command to change a user's password.
    /// This command is processed by the corresponding handler.
    /// </summary>
    public class ChangePasswordCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// Gets or sets the data transfer object (DTO) containing the required information 
        /// for changing the password, including the current and new passwords.
        /// </summary>
        public required ChangePasswordDto Dto { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user requesting the password change.
        /// This ID is used to fetch the user from the identity system.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// The user agent string representing the client's browser or application information.
        /// </summary>
        /// <value>
        /// A string representing the user agent.
        /// </value>
        public string? UserAgent { get; set; }

        /// <summary>
        /// The IP address of the client making the login request.
        /// </summary>
        /// <value>
        /// A string representing the IP address.
        /// </value>
        public string? IpAdress { get; set; }
    }
    internal class ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager,
            MailSettings mailSettings,
            IHttpClientFactory httpClientFactory
        ) : IRequestHandler<ChangePasswordCommand, GenericApiResponse<bool>>
    {
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<GenericApiResponse<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<bool>()
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                response.Message = "User not found";
                response.Statuscode = StatusCodes.Status404NotFound;
                return response;
            }

            if (request.Dto.NewPassword != request.Dto.RepeatNewPassword)
            {
                response.Message = "Passwords do not match";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Dto.CurrentPassword);
            if (!passwordCheck)
            {
                response.Message = "Current password is incorrect";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }

            if (request.Dto.NewPassword == request.Dto.CurrentPassword)
            {
                response.Message = "New password cannot be the same as the current password";
                response.Statuscode = StatusCodes.Status406NotAcceptable;
                return response;
            }
            var result = await _userManager.ChangePasswordAsync(user, request.Dto.CurrentPassword, request.Dto.NewPassword);

            if (result.Succeeded)
            {
                response.Payload = true;
                response.Success = true;
                response.Statuscode = StatusCodes.Status200OK;
                response.Message = "Password changed successfully";

                if (request.UserAgent != null && request.IpAdress != null)
                {
                    var geoInfo = await ExtraMethods.GetGeoLocationInfo(request.IpAdress, _httpClientFactory);
                    await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                    {
                        To = user.Email!,
                        Body = ChangePasswordMail.GetEmailHtml(request.IpAdress, geoInfo?.Country ?? "", request.UserAgent),
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
