using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Mail;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Mails;
using Auth.Infraestructure.Identity.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace Auth.Infraestructure.Identity.Features.Register.Commands.SendValidationEmailAgain
{
    /// <summary>
    /// Command for sending a validation email again to the user in case the user needs to confirm their registration email.
    /// </summary>
    public class SendValidationEmailAgainCommand : IRequest<GenericApiResponse<string>>
    {
        /// <summary>
        /// Contains the request data for the validation email resend, including the user's email.
        /// </summary>
        public required SendValidationEmailAgainRequestDto Dto { get; set; }

        /// <summary>
        /// The origin of the request, typically used for creating the verification URL.
        /// </summary>
        public required string Origin { get; set; }
    }

    internal class SendValidationEmailAgainCommandHandler(UserManager<ApplicationUser> userManager, MailSettings mailSettings) : IRequestHandler<SendValidationEmailAgainCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;


        public async Task<GenericApiResponse<string>> Handle(SendValidationEmailAgainCommand request, CancellationToken cancellationToken)
        {
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
                    response.Statuscode = 404;
                    return response;
                }

                if (user.EmailConfirmed)
                {
                    response.Success = false;
                    response.Message = $"Email {request.Dto.Email} is already confirmed";
                    response.Statuscode = 200;
                    return response;
                }

                var verificationUrl = await ExtraMethods.SendVerificationEMailUrl(user, request.Origin, _userManager);
                await ExtraMethods.SendEmail(_mailSettings, new SendEmailRequestDto
                {
                    To = user.Email!,
                    Body = ValidationEmailMail.GetEmailHtml(verificationUrl),
                    Subject = "Confirm your email"
                });
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
