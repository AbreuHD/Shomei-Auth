using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public bool? IsBanned { get; set; } = false;

        public ICollection<UserProfile>? UserProfile { get; set; } = [];
        public ICollection<UserSession> Sessions { get; set; } = [];
        public ICollection<MailOtp> MailOtp { get; set; } = [];
    }
}
