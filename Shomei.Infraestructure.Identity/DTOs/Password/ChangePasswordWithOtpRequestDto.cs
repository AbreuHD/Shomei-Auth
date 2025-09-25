namespace Shomei.Infraestructure.Identity.DTOs.Password
{
    public class ChangePasswordWithOtpRequestDto
    {
        public required string NewPassword { get; set; }
        public required string RepeatNewPassword { get; set; }
        public required string Otp { get; set; }
        public required string Email { get; set; }
    }
}
