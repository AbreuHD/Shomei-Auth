using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.DTOs.Mail;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Enums;
using Shomei.Infraestructure.Identity.Extra;
using Shomei.Infraestructure.Identity.Mails;
using Shomei.Infraestructure.Identity.Settings;

namespace Shomei.Infraestructure.Identity.Features.Email.Commands
{
    public class RequestEmailChangeOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// Gets or sets the user's current password.
        /// This is required to confirm the user's identity before changing the email.
        /// </summary>
        public required string Password { get; set; }
    }
    internal class RequestEmailChangeOtpCommandHandler(IdentityContext identityContext,
        UserManager<ApplicationUser> userManager,
        MailSettings mailSettings,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory) : IRequestHandler<RequestEmailChangeOtpCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<bool>> Handle(RequestEmailChangeOtpCommand request, CancellationToken cancellationToken)
        {
            var IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";
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
                var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordCheck)
                {
                    response.Message = "Password is incorrect";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    return response;
                }

                var Otp = ExtraMethods.GenerateOtp();
                _identityContext.Set<MailOtp>().Add(new MailOtp
                {
                    UserId = user.Id,
                    Otp = ExtraMethods.GetHash(Otp),
                    Expiration = DateTime.UtcNow.AddMinutes(15),
                    UserAgent = UserAgent,
                    IpAddress = IpAddress,
                    Purpose = OtpPurpose.ChangeEmail.ToString()
                });
                await _identityContext.SaveChangesAsync(cancellationToken);

                var geoInfo = await ExtraMethods.GetGeoLocationInfo(IpAddress, _httpClientFactory);
                var passwordEmail = new PasswordResetEmail
                {
                    UserName = user.UserName!,
                    OtpCode = Otp,
                    Ip = IpAddress,
                    Country = geoInfo?.Country ?? "",
                    UserAgent = UserAgent
                };
                await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                {
                    To = user.Email!,
                    Body = passwordEmail.GetEmailHtml(),
                    Subject = "Confirm your email"
                });

                response.Statuscode = StatusCodes.Status200OK;
                response.Success = true;
                response.Message = "Email change OTP sent successfully.";
                response.Payload = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
