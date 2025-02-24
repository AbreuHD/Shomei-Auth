namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// Data Transfer Object (DTO) for editing a user profile.
    /// </summary>
    /// <remarks>
    /// This object contains the necessary information to update a user's profile, such as their name and avatar URL.
    /// </remarks>
    public class EditUserProfileRequestDto
    {
        /// <summary>
        /// The unique identifier for the user profile being updated.
        /// </summary>
        /// <remarks>
        /// This ID is used to locate the specific user profile that needs to be edited.
        /// </remarks>
        public required int Id { get; set; }

        /// <summary>
        /// The URL of the user's avatar image.
        /// </summary>
        /// <remarks>
        /// This field is optional, and it will update the user's avatar URL if provided.
        /// </remarks>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// The name of the user, typically their full name.
        /// </summary>
        /// <remarks>
        /// This field is required and will update the user's name.
        /// </remarks>
        public required string Name { get; set; }
    }
}
