using Auth.Infraestructure.Identity.Enums;

namespace Auth.Infraestructure.Identity.DTOs.Account
{
    /// <summary>
    /// Data transfer object used to send a request for resending the validation email.
    /// </summary>
    public class SendValidationEmailAgainRequestDto
    {
        /// <summary>
        /// The email address of the user who is requesting 
        /// the validation email to be resent.
        /// </summary>
        public required string Email { get; set; }
    }
}
