using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shomei.Infraestructure.Identity.DTOs.Account;
using Shomei.Infraestructure.Identity.Enums;
using Shomei.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmail;
using Shomei.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmailWithOtp;
using Shomei.Infraestructure.Identity.Features.AuthenticateEmail.Command.GetDataFromJWT;
using Shomei.Infraestructure.Identity.Features.Email.Commands;
using Shomei.Infraestructure.Identity.Features.ForgotPSW.Commands;
using Shomei.Infraestructure.Identity.Features.Login.Queries.AuthLogin;
using Shomei.Infraestructure.Identity.Features.Password.Commads;
using Shomei.Infraestructure.Identity.Features.Register.Commands.CreateAccount;
using Shomei.Infraestructure.Identity.Features.Register.Commands.SendValidationEmailAgain;
using Shomei.Infraestructure.Identity.Features.UserName.Commands;
using Shomei.Infraestructure.Identity.Features.UserProfile.Commands;
using Shomei.Infraestructure.Identity.Features.UserProfile.Queries;
using Shomei.Infraestructure.Identity.Features.UserSessions.Commands;
using Shomei.Infraestructure.Identity.Features.UserSessions.Queries;
using Shomei.Infraestructure.Identity.Features.UserSystem.Queries;
using Shomei.Infraestructure.Identity.Middleware;
using Shomei.Testing.AuthAPI.ExtraConfig.Enums;
using System.Security.Cryptography;

namespace Shomei.Testing.AuthAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController(IMediator mediator) : ControllerBase
    {
        public IMediator Mediator { get; } = mediator;

        [HttpPost("Login")]
        public async Task<IActionResult> AuthLogin([FromBody] AuthLoginQuery request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterAccountRequestDto requestDto)
        {
            var request = new CreateAccountCommand(Roles.User.ToString(), VerificationMode.Otp, false)
            {
                Dto = requestDto,
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] AuthEmailCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("ConfirmEmailWithOtp")]
        public async Task<IActionResult> ConfirmEmailWithOtp([FromQuery] AuthEmailWithOtpCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPost("ResentConfirmation")]
        public async Task<IActionResult> ResentConfirmation([FromBody] SendValidationEmailAgainRequestDto requestDto)
        {

            var request = new SendValidationEmailAgainCommand(VerificationMode.Otp)
            {
                Dto = requestDto,
            };
            var response = await Mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("TestMiddleware")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [MultipleSessionAuthorize]
        [ClaimRequired("ProfileId", "You need select a profile")]
        public IActionResult TestMiddleware()
        {
            var randomGenerator = RandomNumberGenerator.Create();
            byte[] data = new byte[16];
            randomGenerator.GetBytes(data);
            return Ok(BitConverter.ToString(data));
        }

        [HttpPost("SelectProfile")]
        [Authorize]
        public async Task<IActionResult> SelectProfile([FromBody] SelectProfileQuery request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("GetAllProfiles")]
        [Authorize]
        public async Task<IActionResult> GetAllProfiles()
        {
            var response = await Mediator.Send(new GetProfilesQuery());
            return StatusCode(response.Statuscode, response);
        }

        [HttpPost("CreateUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> CreateUserProfile(CreateUserProfileCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpDelete("DeleteUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> DeleteUserProfile(DeleteUserProfileCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("EditUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> EditUserProfile(EditUserProfileCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpDelete("LogoutFromAllSessions")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> LogoutFromAllSessions()
        {
            var response = await Mediator.Send(new LogoutAllSessionsCommand());
            return StatusCode(response.Statuscode, response);
        }

        [HttpDelete("LogoutCurrentSession")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> LogoutCurrentSession()
        {
            var response = await Mediator.Send(new LogoutCurrentSessionCommand());
            return StatusCode(response.Statuscode, response);
        }

        [HttpDelete("LogoutSessionById")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> LogoutSessionById(LogoutSessionByIdCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("GetAllUserSessions")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> GetAllUserSessions()
        {
            var response = await Mediator.Send(new GetAllUserSessionsQuery());
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("ChangePassword")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("ChangeUserName")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> ChangeUserName(ChangeUserNameCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("ChangeEmail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail(ChangeEmailCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }
        [HttpPut("RequestEmailChangeOtp")]
        [Authorize]
        public async Task<IActionResult> RequestEmailChangeOtp(RequestEmailChangeOtpCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }
        [HttpPut("ChangeEmailWithOtp")]
        [Authorize]
        public async Task<IActionResult> ChangeEmailWithOtp(ChangeEmailWithOtpCommand request)
        {
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("GeneratePasswordResetOtp")]
        [Authorize]
        public async Task<IActionResult> GeneratePasswordResetOtp([FromBody] string email)
        {
            var response = await Mediator.Send(new GeneratePasswordResetOtpCommand() { Email = email });
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("GetAllUsers")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await Mediator.Send(new GetAllUsersQuery());
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("GetInfo")]
        [Authorize]
        public async Task<IActionResult> GetInfo()
        {
            var response = await Mediator.Send(new GetDataFromJwtCommand());
            return StatusCode(response.Statuscode, response);
        }
    }
}
