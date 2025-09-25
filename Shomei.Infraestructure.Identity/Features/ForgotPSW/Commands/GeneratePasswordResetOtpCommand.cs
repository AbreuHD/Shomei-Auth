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

namespace Shomei.Infraestructure.Identity.Features.ForgotPSW.Commands
{
    public class GeneratePasswordResetOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        public required string Email { get; set; }
    }
    internal class GeneratePasswordResetOtpCommandHandler(IdentityContext identityContext,
        UserManager<ApplicationUser> userManager,
        MailSettings mailSettings,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory) : IRequestHandler<GeneratePasswordResetOtpCommand, GenericApiResponse<bool>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IdentityContext _identityContext = identityContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<GenericApiResponse<bool>> Handle(GeneratePasswordResetOtpCommand request, CancellationToken cancellationToken)
        {
            var IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";

            var response = new GenericApiResponse<bool>()
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    response.Message = "User with that email not found.";
                    response.Statuscode = StatusCodes.Status404NotFound;
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
                    Purpose = OtpPurpose.PasswordReset.ToString()
                });
                await _identityContext.SaveChangesAsync(cancellationToken);

                var geoInfo = await ExtraMethods.GetGeoLocationInfo(IpAddress, _httpClientFactory);
                await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                {
                    To = user.Email!,
                    Body = ChangeEmailOtp.GetEmailHtml(user.UserName!, Otp, IpAddress, geoInfo?.Country ?? "", UserAgent),
                    Subject = "Password Reset"
                });

                response.Statuscode = StatusCodes.Status200OK;
                response.Success = true;
                response.Message = "Password Reset OTP sent successfully.";
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
