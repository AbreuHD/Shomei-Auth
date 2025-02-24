using Newtonsoft.Json;

namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// Response DTO for authentication containing the user's details and JWT token.
    /// </summary>
    public class AuthenticationResponse
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The first name of the user.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The last name of the user.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// The username of the user.
        /// </summary>
        public required string UserName { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// The roles assigned to the user.
        /// </summary>
        public required List<string> Roles { get; set; }

        /// <summary>
        /// Whether the user's email is verified.
        /// </summary>
        public required bool IsVerified { get; set; }

        /// <summary>
        /// The JWT token used for authenticating the user.
        /// </summary>
        public required string JWToken { get; set; }

        /// <summary>
        /// The refresh token used to obtain a new JWT token.
        /// </summary>
        [JsonIgnore]
        public string? RefreshToken { get; set; }
    }
}
