using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Mail;
using Auth.Infraestructure.Identity.DTOs.Otp;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Mails;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    public class RequestEmailChangeOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        public required EmailChangeOtpRequestDto Dto { get; set; }
        public required string UserId { get; set; }
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
    }
    internal class RequestEmailChangeOtpCommandHandler(IdentityContext identityContext,
        UserManager<ApplicationUser> userManager,
        MailSettings mailSettings,
        IHttpClientFactory httpClientFactory) : IRequestHandler<RequestEmailChangeOtpCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<GenericApiResponse<bool>> Handle(RequestEmailChangeOtpCommand request, CancellationToken cancellationToken)
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
                var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Dto.Password);
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
                    Otp = ExtraMethods.HashToken(Otp),
                    ExpirationDate = DateTime.UtcNow.AddMinutes(15),
                    UserAgent = request.UserAgent,
                    IpAddress = request.IpAddress
                });
                await _identityContext.SaveChangesAsync(cancellationToken);

                var geoInfo = await ExtraMethods.GetGeoLocationInfo(request.IpAddress, _httpClientFactory);
                await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                {
                    To = user.Email!,
                    Body = ChangeEmailOtp.GetEmailHtml(user.UserName!, Otp, request.IpAddress, geoInfo?.Country ?? "", request.UserAgent),
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
