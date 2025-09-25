using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Mail;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Mails;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;

namespace Auth.Infraestructure.Identity.Features.Register.Commands.SendValidationEmailAgain
{
    /// <summary>
    /// Command for sending a validation email again to the user in case the user needs to confirm their registration email.
    /// </summary>
    public class SendValidationEmailAgainCommand(VerificationMode VerificationMode) : IRequest<GenericApiResponse<string>>
    {
        public required SendValidationEmailAgainRequestDto Dto { get; set; }

        /// <summary>
        /// Specifies the verification mode (e.g., account confirmation, 
        /// password reset, or other verification purposes).
        /// </summary>
        public VerificationMode VerificationMode { get; } = VerificationMode;
        /// <summary>
        /// The origin URL of the request (usually the client app’s base URL),
        /// which can be used to generate confirmation links.
        /// </summary>
        public string? Origin { get; set; }
    }

    internal class SendValidationEmailAgainCommandHandler(UserManager<ApplicationUser> userManager, MailSettings mailSettings, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IdentityContext identityContext) : IRequestHandler<SendValidationEmailAgainCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        public async Task<GenericApiResponse<string>> Handle(SendValidationEmailAgainCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Origin))
            {
                request.Origin = _httpContextAccessor.HttpContext!.Request.Headers.Origin.ToString() ?? "Unknown";
            }
            var UserAgent = _httpContextAccessor.HttpContext!.Request.Headers.UserAgent.ToString() ?? "Unknown";
            var IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var response = new GenericApiResponse<string>()
            {
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = "N/A"
            };
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Dto.Email);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = $"Email {request.Dto.Email} not exist";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    return response;
                }
                if (user.EmailConfirmed)
                {
                    response.Success = false;
                    response.Message = $"Email {request.Dto.Email} is already confirmed";
                    response.Statuscode = StatusCodes.Status200OK;
                    return response;
                }
                switch (request.VerificationMode)
                {
                    case VerificationMode.Otp:
                        var Otp = ExtraMethods.GenerateOtp();
                        _identityContext.Set<MailOtp>().Add(new MailOtp
                        {
                            UserId = user.Id,
                            Otp = ExtraMethods.GetHash(Otp),
                            Expiration = DateTime.UtcNow.AddMinutes(15),
                            UserAgent = UserAgent,
                            IpAddress = IpAddress,
                            Purpose = OtpPurpose.VerifyAccount.ToString()
                        });
                        await _identityContext.SaveChangesAsync(cancellationToken);
                        var geoInfo = await ExtraMethods.GetGeoLocationInfo(IpAddress, _httpClientFactory);
                        await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                        {
                            To = user.Email!,
                            Body = ChangeEmailOtp.GetEmailHtml(user.UserName!, Otp, IpAddress, geoInfo?.Country ?? "", UserAgent),
                            Subject = "Confirm your Email"
                        });
                        response.Message = "We send a verification Otp to your account, please active your account";
                        break;
                    case VerificationMode.EmailLink:
                        var verificationUrl = await ExtraMethods.SendVerificationEMailUrl(user, request.Origin, _userManager);
                        await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                        {
                            To = user.Email!,
                            Body = ValidationEmailMail.GetEmailHtml(verificationUrl),
                            Subject = "Confirm your email"
                        });
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Statuscode = 500;
                return response;
            }
            response.Success = true;
            response.Message = $"Email to {request.Dto.Email} sent";
            response.Statuscode = 200;
            return response;
        }
    }
}