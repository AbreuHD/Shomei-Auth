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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Net.Http;

namespace Auth.Infraestructure.Identity.Features.Register.Commands.CreateAccount
{
    /// <summary>
    /// Represents a command to create a new user account.
    /// This command checks for the availability of the username and email, 
    /// creates the user in the system, assigns the appropriate role, 
    /// and sends a confirmation email with a verification link.
    /// </summary>
    public class CreateAccountCommand(string userType, VerificationMode VerificationMode, bool useUsername = true) : IRequest<GenericApiResponse<string>>
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

        public bool UseUsername { get; } = useUsername;
    }
    internal class CreateAccountCommandHandler(UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IdentityContext identityContext,
        IHttpClientFactory httpClientFactory,
        MailSettings mailSettings) : IRequestHandler<CreateAccountCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly MailSettings _mailSettings = mailSettings;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        public async Task<GenericApiResponse<string>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Origin))
            {
                request.Origin = _httpContextAccessor.HttpContext!.Request.Headers.Origin.ToString() ?? "Unknown";
            }
            var UserAgent = _httpContextAccessor.HttpContext!.Request.Headers.UserAgent.ToString() ?? "Unknown";
            var IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var response = new GenericApiResponse<string>()
            {
                Payload = "Account Created",
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "We send a verification Email to your account, please active your account"
            };
            try
            {
                ApplicationUser? UserNameExist = null;

                if (!request.UseUsername)
                {
                    do
                    {
                        request.Dto.UserName = ExtraMethods.GenerateRandomUsername(request.Dto.Name, request.Dto.LastName);
                        UserNameExist = await _userManager.FindByNameAsync(request.Dto.UserName);
                    } while (UserNameExist != null);
                }
                else
                {
                    UserNameExist = await _userManager.FindByNameAsync(request.Dto.UserName);
                }

                if (string.IsNullOrEmpty(request.Dto.Email) && !request.UseUsername)
                {
                    response.Message = "You need to complete all the values";
                    response.Statuscode = StatusCodes.Status500InternalServerError;
                    response.Success = false;
                    return response;
                }

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

                switch(request.VerificationMode)
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
                response.Statuscode = StatusCodes.Status500InternalServerError;
                response.Payload = string.Empty;
            }
            return response;
        }
    }
}
