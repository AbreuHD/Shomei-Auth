using Auth.Infraestructure.Identity.Enums;

namespace Auth.Infraestructure.Identity.Entities
{
    public class MailOtp
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Otp { get; set; }
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
        public required DateTime Expiration { get; set; }
        public required string Purpose { get; set; }

        public bool? Used { get; set; } = false;

        public ApplicationUser? User { get; set; }
    }
}
