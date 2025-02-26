namespace Auth.Infraestructure.Identity.DTOs.Email
{
    public class SendEmailPartialRequestDto
    {
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
