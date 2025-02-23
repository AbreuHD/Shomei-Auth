using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Features.Email.Commands.SendEmail;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace Auth.Infraestructure.Identity.Features.Register.Commands.SendValidationEmailAgain
{
    public class SendValidationEmailAgainCommand : IRequest<GenericApiResponse<string>>
    {
        public required SendValidationEmailAgainRequestDto Dto { get; set; }
        public required string Origin { get; set; }
    }

    internal class SendValidationEmailAgainCommandHandler(UserManager<ApplicationUser> userManager, IMediator mediator) : IRequestHandler<SendValidationEmailAgainCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private IMediator Mediator { get; } = mediator;

        public async Task<GenericApiResponse<string>> Handle(SendValidationEmailAgainCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<string>();
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
                await Mediator.Send(new SendEmailCommand
                {
                    To = user.Email,
                    Body = $"Please confirm your account visiting this URL {verificationUrl}",
                    Subject = "Confirm registration"
                }, cancellationToken);
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
