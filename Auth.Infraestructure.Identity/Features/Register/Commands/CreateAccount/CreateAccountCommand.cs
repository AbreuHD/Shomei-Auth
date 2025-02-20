using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Auth.Infraestructure.Identity.Features.Email.Commands.SendEmail;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Register.Commands.CreateAccount
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateAccountCommand(string userType) : IRequest<GenericApiResponse<string>>
    {
        public string UserType { get; } = userType;

        public required RegisterAccountRequestDto Dto { get; set; }
        public required string ORIGIN { get; set; }
    }
    internal class CreateAccountCommandHandler(UserManager<ApplicationUser> userManager, IMediator mediator) : IRequestHandler<CreateAccountCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private IMediator Mediator { get; } = mediator;

        public async Task<GenericApiResponse<string>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<string>();
            try
            {
                var UserNameExist = await _userManager.FindByNameAsync(request.Dto.UserName);

                if (UserNameExist != null)
                {
                    response.Success = false;
                    response.Message = $"Username {request.Dto.UserName} is already taken";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    response.Payload = string.Empty;
                    return response;
                }

                var EmailExist = await _userManager.FindByEmailAsync(request.Dto.Email);
                if (EmailExist != null)
                {
                    response.Success = false;
                    response.Message = $"Email {request.Dto.Email} is already registered";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    response.Payload = string.Empty;
                    return response;
                }

                var user = new ApplicationUser
                {
                    Email = request.Dto.Email,
                    Name = request.Dto.Name,
                    LastName = request.Dto.LastName,
                    UserName = request.Dto.UserName,
                    PhoneNumber = request.Dto.Phone,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, request.Dto.Password);
                if (!result.Succeeded)
                {
                    response.Success = false;
                    response.Message = "A error occurred trying to register the user.";
                    response.Statuscode = StatusCodes.Status400BadRequest;
                    response.Payload = string.Empty;
                    return response;
                }

                var regiteredUser = await _userManager.FindByEmailAsync(user.Email);
                response.Payload = regiteredUser!.Id;
                await _userManager.AddToRoleAsync(user, request.UserType);

                var verificationUrl = await ExtraMethods.SendVerificationEMailUrl(user, request.ORIGIN, _userManager);

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
                response.Statuscode = StatusCodes.Status500InternalServerError;
                response.Payload = string.Empty;
            }
            return response;
        }
    }
}
