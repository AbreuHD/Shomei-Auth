namespace Shomei.Infraestructure.Identity.Mails
{
    public class PasswordResetEmail
    {
        public required string UserName { get; set; }
        public required string OtpCode { get; set; }
        public required string Ip { get; set; }
        public required string Country { get; set; }
        public required string UserAgent { get; set; }

        public virtual string GetEmailHtml()
        {
            return $@"<!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Password Reset Request</title>
                        </head>
                        <body>
                            <h2>Password Reset Request</h2>
                            <p>Hello {UserName},</p>
                            <p>You have requested to reset your password. Use the following One-Time Password (OTP) to proceed:</p>
                            <p><strong>Your OTP Code:</strong> {OtpCode}</p>
                            <p><strong>Request Details:</strong></p>
                            <ul>
                                <li><strong>IP Address:</strong> {Ip}</li>
                                <li><strong>Location:</strong> {Country}</li>
                                <li><strong>Browser and OS:</strong> {UserAgent}</li>
                            </ul>
                            <p>If you did not request this, please ignore this email or contact our support team.</p>
                            <p>&copy; 2025 AbreuHD. All rights reserved.</p>
                        </body>
                        </html>";
        }
    }
}
