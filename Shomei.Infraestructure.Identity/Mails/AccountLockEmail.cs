namespace Shomei.Infraestructure.Identity.Mails
{
    public class AccountLockEmail
    {
        public required string UserName { get; set; }
        public required string Ip { get; set; }
        public required string Country { get; set; }
        public required string UserAgent { get; set; }
        public required string LockDuration { get; set; }

        public virtual string GetEmailHtml()
        {
            return $@"<!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Account Locked</title>
                        </head>
                        <body>
                            <h2>Account Locked Due to Multiple Failed Login Attempts</h2>
                            <p>Hello {UserName},</p>
                            <p>Your account has been temporarily locked due to multiple unsuccessful login attempts.</p>
                            <p><strong>Lock Duration:</strong> {LockDuration}</p>
                            <p><strong>Request Details:</strong></p>
                            <ul>
                                <li><strong>IP Address:</strong> {Ip}</li>
                                <li><strong>Location:</strong> {Country}</li>
                                <li><strong>Browser and OS:</strong> {UserAgent}</li>
                            </ul>
                            <p>If this was not you, we recommend resetting your password immediately once the lock period expires.</p>
                            <p>If you need assistance, please contact our support team.</p>
                            <p>&copy; 2025 AbreuHD. All rights reserved.</p>
                        </body>
                        </html>";
        }
    }
}
