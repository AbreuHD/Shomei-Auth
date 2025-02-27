using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Email;
using Auth.Infraestructure.Identity.DTOs.Otp;
using Auth.Infraestructure.Identity.DTOs.Password;
using Auth.Infraestructure.Identity.DTOs.UserName;
using Auth.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmail;
using Auth.Infraestructure.Identity.Features.Email.Commands;
using Auth.Infraestructure.Identity.Features.Login.Queries.AuthLogin;
using Auth.Infraestructure.Identity.Features.Password.Commads;
using Auth.Infraestructure.Identity.Features.Register.Commands.CreateAccount;
using Auth.Infraestructure.Identity.Features.Register.Commands.SendValidationEmailAgain;
using Auth.Infraestructure.Identity.Features.UserName.Commands;
using Auth.Infraestructure.Identity.Features.UserProfile.Commands;
using Auth.Infraestructure.Identity.Features.UserProfile.Queries;
using Auth.Infraestructure.Identity.Features.UserSessions.Commands;
using Auth.Infraestructure.Identity.Features.UserSessions.Queries;
using Auth.Infraestructure.Identity.Middleware;
using Auth.Testing.AuthAPI.ExtraConfig.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Auth.Testing.AuthAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController(IMediator mediator) : ControllerBase
    {
        public IMediator Mediator { get; } = mediator;

        [HttpPost("Login")]
        public async Task<IActionResult> AuthLogin([FromBody] LoginRequestDto requestDto)
        {
            var request = new AuthLoginQuery
            {
                Dto = requestDto,
                UserAgent = Request.Headers.UserAgent.ToString(),
                IpAdress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterAccountRequestDto requestDto)
        {
            var request = new CreateAccountCommand(Roles.User.ToString())
            {
                Dto = requestDto,
                Origin = Request.Headers.Origin.ToString() ?? "Unknown"
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequestDto requestDto)
        {
            var request = new AuthEmailCommand
            {
                Dto = requestDto
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPost("ResentConfirmation")]
        public async Task<IActionResult> ResentConfirmation([FromBody] SendValidationEmailAgainRequestDto requestDto)
        {

            var request = new SendValidationEmailAgainCommand
            {
                Dto = requestDto,
                Origin = Request.Headers.Origin.ToString() ?? "Unknown"
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
        public async Task<IActionResult> SelectProfile([FromBody] SelectProfileRequestDto requestDto)
        {
            var request = new SelectProfileQuery
            {
                Dto = requestDto,
                UserAgent = Request.Headers.UserAgent.ToString(),
                IpAdress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserId = User.FindFirst("uid")!.Value
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpGet("GetAllProfiles")]
        [Authorize]
        public async Task<IActionResult> GetAllProfiles()
        {
            var response = await Mediator.Send(new GetProfilesQuery { UserId = User.FindFirst("uid")!.Value });
            return StatusCode(response.Statuscode, response);
        }

        [HttpPost("CreateUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> CreateUserProfile(CreateUserProfileRequestDto requestDto)
        {
            var request = new CreateUserProfileCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpDelete("DeleteUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> DeleteUserProfile(int Id)
        {
            var request = new DeleteUserProfileCommand() { Id = Id, UserId = User.FindFirst("uid")!.Value };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("EditUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> EditUserProfile(EditUserProfileRequestDto requestDto)
        {
            var request = new EditUserProfileCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value,
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpDelete("LogoutFromAllSessions")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> LogoutFromAllSessions()
        {
            var request = new LogoutAllSessionsCommand()
            {
                UserId = User.FindFirst("uid")!.Value,
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpDelete("LogoutCurrentSession")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> LogoutCurrentSession()
        {
            var request = new LogoutCurrentSessionCommand()
            {
                Token = Request.Headers.Authorization.ToString().Split(" ")[1],
            };
            var response = await Mediator.Send(request);
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
            var request = new GetAllUserSessionsQuery()
            {
                UserId = User.FindFirst("uid")!.Value,
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("ChangePassword")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto requestDto)
        {
            var request = new ChangePasswordCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value,
                IpAdress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = Request.Headers.UserAgent.ToString()
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("ChangeUserName")]
        [MultipleSessionAuthorize]
        public async Task<IActionResult> ChangeUserName(ChangeUserNameRequestDto requestDto)
        {
            var request = new ChangeUserNameCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value,
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("ChangeEmail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail(ChangeEmailRequestDto requestDto)
        {
            var request = new ChangeEmailCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value,
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("RequestEmailChangeOtp")]
        [Authorize]
        public async Task<IActionResult> RequestEmailChangeOtp(EmailChangeOtpRequestDto requestDto)
        {
            var request = new RequestEmailChangeOtpCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = Request.Headers.UserAgent.ToString()
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("ChangeEmailWithOtp")]
        [Authorize]
        public async Task<IActionResult> ChangeEmailWithOtp(ChangeEmailRequestDto requestDto)
        {
            var request = new ChangeEmailCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value,
                UseOtp = true
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }
    }
}
