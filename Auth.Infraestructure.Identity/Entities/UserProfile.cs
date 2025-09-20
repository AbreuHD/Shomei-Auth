using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Infraestructure.Identity.Entities
{
    public class UserProfile
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Password { get; set; }
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
