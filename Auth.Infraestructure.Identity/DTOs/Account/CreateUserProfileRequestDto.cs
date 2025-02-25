namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating a user profile.
    /// </summary>
    /// <remarks>
    /// This DTO is used to transfer the data necessary for creating a user profile. 
    /// It contains the user's name and an optional avatar URL that can be used to personalize the user's profile.
    /// </remarks>
    public class CreateUserProfileRequestDto
    {
        /// <summary>
        /// The full name of the user.
        /// </summary>
        /// <remarks>
        /// The user's name is a required field and will be used to identify the user on the platform.
        /// </remarks>
        public required string Name { get; set; }

        /// <summary>
        /// The URL to the user's avatar image.
        /// </summary>
        /// <remarks>
        /// This field is optional. If provided, it will be used to display the user's profile picture. 
        /// If not provided, the user will have a default avatar.
        /// </remarks>
        public string? AvatarUrl { get; set; }
    }
}
