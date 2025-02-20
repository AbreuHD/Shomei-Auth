namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class ResetPasswordRequestDto
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
