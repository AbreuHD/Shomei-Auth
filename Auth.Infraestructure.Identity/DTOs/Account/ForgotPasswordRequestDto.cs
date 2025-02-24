namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// DTO for the password recovery request.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        /// The email of the user requesting password recovery.
        /// </summary>
        public required string Email { get; set; }
    }
}
