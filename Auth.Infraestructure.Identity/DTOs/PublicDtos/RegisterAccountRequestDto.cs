namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class RegisterAccountRequestDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public required string Phone { get; set; }
    }
}
