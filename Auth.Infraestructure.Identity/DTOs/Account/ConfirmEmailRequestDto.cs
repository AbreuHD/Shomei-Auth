namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// Data Transfer Object (DTO) used to carry the information required for email confirmation.
    /// It includes the user's ID and the email confirmation token.
    /// </summary>
    public class ConfirmEmailRequestDto
    {
        /// <summary>
        /// The unique identifier of the user whose email is being confirmed.
        /// </summary>
        /// <value>
        /// A string representing the user ID, which is required for email confirmation.
        /// </value>
        public required string UserId { get; set; }

        /// <summary>
        /// The confirmation token sent to the user for verifying their email address.
        /// </summary>
        /// <value>
        /// A string representing the token that will be used to confirm the user's email.
        /// </value>
        public required string Token { get; set; }
    }
}
