namespace Auth.Infraestructure.Identity.DTOs.Password
{
    /// <summary>
    /// Data transfer object (DTO) for changing a user's password.
    /// Contains the necessary fields to validate and update the password.
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// Gets or sets the current password of the user.
        /// This is required to verify the user's identity before changing the password.
        /// </summary>
        public required string CurrentPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password that the user wants to set.
        /// Must meet the system's password security requirements.
        /// </summary>
        public required string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the repeated entry of the new password.
        /// Used to confirm that the user has entered the intended new password correctly.
        /// </summary>
        public required string RepeatNewPassword { get; set; }
    }
}
