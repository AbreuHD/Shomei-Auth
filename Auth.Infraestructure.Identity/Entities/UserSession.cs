using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Infraestructure.Identity.Entities
{
    public class UserSession
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public required DateTime Expiration { get; set; }
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
        public required DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
