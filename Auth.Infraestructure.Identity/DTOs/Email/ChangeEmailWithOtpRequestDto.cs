namespace Shomei.Infraestructure.Identity.DTOs.Email
{
    public class ChangeEmailWithOtpRequestDto
    {
        public required string NewEmail { get; set; }
        public required string Otp { get; set; }
    }
}