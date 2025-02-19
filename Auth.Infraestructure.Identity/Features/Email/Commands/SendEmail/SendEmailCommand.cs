using Auth.Infraestructure.Identity.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using MimeKit;

namespace Auth.Infraestructure.Identity.Features.Email.Commands.SendEmail
{
    public class SendEmailCommand : IRequest<bool>
    {
        public required string To { get; set; }
        public required string Subject { get; set; }
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
