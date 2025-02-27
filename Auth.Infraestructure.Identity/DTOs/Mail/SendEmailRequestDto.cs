namespace Auth.Infraestructure.Identity.DTOs.Mail
{
    public class SendEmailRequestDto
    {
        /// <summary>
        /// The recipient's email address.
        /// </summary>
        /// <value>
        /// A string representing the email address of the recipient.
        /// </value>
        public required string To { get; set; }

        /// <summary>
        /// The subject of the email.
        /// </summary>
        /// <value>
        /// A string representing the subject line of the email.
        /// </value>
        public required string Subject { get; set; }

        /// <summary>
        /// The body content of the email.
        /// </summary>
        /// <value>
        /// A string representing the body content of the email, typically in HTML format.
        /// </value>
        public required string Body { get; set; }
    }
}
