﻿using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Mail;
using Auth.Infraestructure.Identity.DTOs.Password;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Mails;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.ForgotPSW.Commands
{
    public class GeneratePasswordResetOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        public required PasswordChangeOtpRequestDto Dto { get; set; }
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
    }
    internal class GeneratePasswordResetOtpCommandHandler(IdentityContext identityContext,
        UserManager<ApplicationUser> userManager,
        MailSettings mailSettings,
        IHttpClientFactory httpClientFactory) : IRequestHandler<GeneratePasswordResetOtpCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<GenericApiResponse<bool>> Handle(GeneratePasswordResetOtpCommand request, CancellationToken cancellationToken)
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
                var user = await _userManager.FindByEmailAsync(request.Dto.Email);
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
                    Otp = ExtraMethods.HashToken(Otp),
                    Expiration = DateTime.UtcNow.AddMinutes(15),
                    UserAgent = request.UserAgent,
                    IpAddress = request.IpAddress,
                    Purpose = OtpPurpose.PasswordReset.ToString()
                });
                await _identityContext.SaveChangesAsync(cancellationToken);

                var geoInfo = await ExtraMethods.GetGeoLocationInfo(request.IpAddress, _httpClientFactory);
                await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                {
                    To = user.Email!,
                    Body = ChangeEmailOtp.GetEmailHtml(user.UserName!, Otp, request.IpAddress, geoInfo?.Country ?? "", request.UserAgent),
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
