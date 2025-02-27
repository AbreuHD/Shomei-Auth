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

namespace Auth.Infraestructure.Identity.Features.Register.Commands.CreateAccount
{
    /// <summary>
    /// Represents a command to create a new user account.
    /// This command checks for the availability of the username and email, 
    /// creates the user in the system, assigns the appropriate role, 
    /// and sends a confirmation email with a verification link.
    /// </summary>
    public class CreateAccountCommand(string userType) : IRequest<GenericApiResponse<string>>
    {
        /// <summary>
        /// The data transfer object (DTO) that contains the necessary information for creating the account.
        /// </summary>
        /// <value>
        /// A <see cref="RegisterAccountRequestDto"/> that contains the user's registration details like name, email, username, password, etc.
        /// </value>
        public required RegisterAccountRequestDto Dto { get; set; }

        /// <summary>
        /// The type of user (e.g., admin, regular user).
        /// This determines the role assigned to the newly created account.
        /// </summary>
        /// <value>
        /// A string representing the user type (role) to assign to the new user.
        /// </value>
        public string UserType { get; } = userType;

        // <summary>
        /// The origin URL where the user will be redirected after successful email verification.
        /// </summary>
        /// <value>
        /// A string representing the origin URL for the verification email.
        /// </value>
        public required string Origin { get; set; }
    }
    internal class CreateAccountCommandHandler(UserManager<ApplicationUser> userManager, MailSettings mailSettings) : IRequestHandler<CreateAccountCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;

        public async Task<GenericApiResponse<string>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<string>()
            {
                Payload = "N/A",
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = "N/A"
            };
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

                if (request.Dto.Password != request.Dto.ConfirmPassword)
                {
                    response.Success = false;
                    response.Message = "Passwords do not match";
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
                response.Statuscode = StatusCodes.Status500InternalServerError;
                response.Payload = string.Empty;
            }
            return response;
        }
    }
}
