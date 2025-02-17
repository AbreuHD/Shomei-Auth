using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Infraestructure.Identity.Entities
{
    public class UserSession
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public required DateTime Expiration { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
