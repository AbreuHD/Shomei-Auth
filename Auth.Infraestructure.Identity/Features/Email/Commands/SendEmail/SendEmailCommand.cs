using Auth.Infraestructure.Identity.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using MimeKit;

namespace Auth.Infraestructure.Identity.Features.Email.Commands.SendEmail
{
    /// <summary>
    /// Represents a command to send an email.
    /// This command carries the necessary details to compose and send an email using SMTP.
    /// </summary>
    public class SendEmailCommand : IRequest<bool>
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
    internal class SendEmailCommandHandler(MailSettings mailSettings) : IRequestHandler<SendEmailCommand, bool>
    {
        private MailSettings MailSettings { get; } = mailSettings;

        public async Task<bool> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse($"{MailSettings.DisplayName} <{MailSettings.EmailFrom}>")
                };
                email.From.Add(new MailboxAddress(MailSettings.DisplayName, MailSettings.EmailFrom));
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                BodyBuilder bodyBuilder = new()
                {
                    HtmlBody = request.Body
                };
                email.Body = bodyBuilder.ToMessageBody();

                using SmtpClient smtp = new();
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtp.ConnectAsync(MailSettings.SmtpHost, MailSettings.SmtpPort, SecureSocketOptions.SslOnConnect, cancellationToken);
                await smtp.AuthenticateAsync(MailSettings.SmtpUser, MailSettings.SmtpPassword, cancellationToken);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true, cancellationToken);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
