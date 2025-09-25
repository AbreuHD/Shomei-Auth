namespace Shomei.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// DTO for the reset password request.
    /// </summary>
    public class ResetPasswordRequestDto
    {
        /// <summary>
        /// The email of the user requesting password reset.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// The token generated for the password reset process.
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// The new password to set for the user.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// The confirmation of the new password.
        /// </summary>
        public required string ConfirmPassword { get; set; }
    }
}
