namespace Auth.Infraestructure.Identity.DTOs.Email
{
    public class ChangeEmailRequestDto
    {
        public required string NewEmail { get; set; }
        public required string Password { get; set; }
    }
}
