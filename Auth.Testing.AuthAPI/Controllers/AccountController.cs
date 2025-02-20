using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmail;
using Auth.Infraestructure.Identity.Features.Login.Queries.AuthLogin;
using Auth.Infraestructure.Identity.Features.Register.Commands.CreateAccount;
using Auth.Infraestructure.Identity.Features.Register.Commands.SendValidationEmailAgain;
using Auth.Infraestructure.Identity.Features.UserProfile.Commands;
using Auth.Infraestructure.Identity.Features.UserProfile.Queries;
using Auth.Infraestructure.Identity.Middleware;
using Auth.Testing.AuthAPI.ExtraConfig.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Testing.AuthAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController(IMediator mediator) : ControllerBase
    {
        public IMediator Mediator { get; } = mediator;

        [HttpGet("Login")]
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
                ORIGIN = Request.Headers.Origin.ToString() ?? "Unknown"
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
        [ClaimRequired("ProfileId", "You need select a profile")]
        public async Task<IActionResult> TestMiddleware()
        {
            var rng = new Random().Next();
            return Ok(rng);
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
        public async Task<IActionResult> CreateUserProfileCommand(CreateUserProfileRequestDto requestDto)
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
        public async Task<IActionResult> DeleteUserProfileCommand(int Id)
        {
            var request = new DeleteUserProfileCommand() { Id = Id, UserId = User.FindFirst("uid")!.Value };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }

        [HttpPut("EditUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> EditUserProfileCommand(EditUserProfileRequestDto requestDto)
        {
            var request = new EditUserProfileCommand()
            {
                Dto = requestDto,
                UserId = User.FindFirst("uid")!.Value,
            };
            var response = await Mediator.Send(request);
            return StatusCode(response.Statuscode, response);
        }
    }
}
