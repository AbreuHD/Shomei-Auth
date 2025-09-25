namespace Shomei.Infraestructure.Identity.DTOs.UserName
{
    /// <summary>
    /// Data transfer object (DTO) for changing a user's username.
    /// Contains the necessary fields to validate and update the username.
    /// </summary>
    public class ChangeUserNameRequestDto
    {
        /// <summary>
        /// Gets or sets the current password of the user.
        /// This is required to verify the user's identity before allowing the username change.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Gets or sets the new username that the user wants to set.
        /// Must meet the system's username validation requirements.
        /// </summary>
        public required string NewUserName { get; set; }
    }
}
