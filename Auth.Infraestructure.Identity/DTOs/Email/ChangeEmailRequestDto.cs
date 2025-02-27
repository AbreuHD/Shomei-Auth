namespace Auth.Infraestructure.Identity.DTOs.Email
{
    public class ChangeEmailRequestDto
    {
        public required string NewEmail { get; set; }
        public string? Password { get; set; }
        public string? Otp { get; set; }
    }
}
