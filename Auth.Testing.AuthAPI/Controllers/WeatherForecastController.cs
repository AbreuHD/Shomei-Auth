using Auth.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmail;
using Auth.Infraestructure.Identity.Features.Login.Queries.AuthLogin;
using Auth.Infraestructure.Identity.Features.Register.Commands.CreateAccount;
using Auth.Infraestructure.Identity.Features.Register.Commands.SendValidationEmailAgain;
using Auth.Infraestructure.Identity.Features.UserProfile.Commands;
using Auth.Infraestructure.Identity.Features.UserProfile.Queries;
using Auth.Infraestructure.Identity.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Testing.AuthAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public IMediator Mediator { get; }
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IMediator mediator, ILogger<WeatherForecastController> logger)
        {
            Mediator = mediator;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> AuthLogin([FromBody] AuthLoginQuery request)
        {
            var data = await Mediator.Send(request);
            return Ok(data);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateAccountCommand request)
        {
            var data = await Mediator.Send(request);
            return Ok(data);
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] AuthEmailCommand request)
        {
            var data = await Mediator.Send(request);
            return Ok(data);
        }

        [HttpPost("ResentConfirmation")]
        public async Task<IActionResult> ResentConfirmation([FromBody] SendValidationEmailAgainCommand request)
        {
            var data = await Mediator.Send(request);
            return Ok(data);
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
        public async Task<IActionResult> SelectProfile([FromBody] SelectProfileQuery request)
        {
            var userId = User.FindFirst("uid")!.Value;
            request.UserId = userId;
            var data = await Mediator.Send(request);
            return Ok(data);
        }

        [HttpPost("GetAllProfiles")]
        [Authorize]
        public async Task<IActionResult> GetAllProfiles()
        {
            var userId = User.FindFirst("uid")!.Value;
            var data = await Mediator.Send(new GetProfilesQuery { UserId = userId });
            return Ok(data);
        }

        [HttpPost("CreateUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> CreateUserProfileCommand(CreateUserProfileCommand request)
        {
            var userId = User.FindFirst("uid")!.Value;
            request.UserId = userId;
            var data = await Mediator.Send(request);
            return Ok(data);
        }

        [HttpPost("DeleteUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> DeleteUserProfileCommand(int Id)
        {
            var userId = User.FindFirst("uid")!.Value;
            var request = new DeleteUserProfileCommand() { Id = Id, UserId = userId };
            var data = await Mediator.Send(request);
            return Ok(data);
        }

        [HttpPost("EditUserProfileCommand")]
        [Authorize]
        public async Task<IActionResult> EditUserProfileCommand(EditUserProfileCommand request)
        {
            var userId = User.FindFirst("uid")!.Value;
            request.UserId = userId;
            var data = await Mediator.Send(request);
            return Ok(data);
        }
    }
}
