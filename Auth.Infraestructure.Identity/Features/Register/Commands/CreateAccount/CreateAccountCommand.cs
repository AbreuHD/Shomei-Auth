using MediatR;

namespace Auth.Infraestructure.Identity.Features.Register.Commands.CreateAccount
{
    public class CreateAccountCommand : IRequest<string>
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string ImageProfile { get; set; } = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public required string Phone { get; set; }
    }
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, string>
    {
        
    }

}
