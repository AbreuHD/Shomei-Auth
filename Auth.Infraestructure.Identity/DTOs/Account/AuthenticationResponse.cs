using System.Text.Json.Serialization;

namespace Auth.Infraestructure.Identity.DTOs.Account
{
    public class AuthenticationResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }

        public required List<string> Roles { get; set; }
        public required bool IsVerified { get; set; }

        public required string JWToken { get; set; }
        [JsonIgnore]
        public required string RefreshToken { get; set; }
    }
}
