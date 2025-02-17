using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }

        public ICollection<UserProfile>? UserProfile { get; set; } = [];
    }
}
