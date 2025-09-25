namespace Shomei.Infraestructure.Identity.Mails
{
    public static class ChangeEmailOtp
    {
        public static string GetEmailHtml(string UserName, string OtpCode, string Ip, string Country, string UserAgent)
        {
            return $@"<!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>OTP Verification</title>
                    </head>
                    <body>
                        <h2>OTP Verification</h2>
                        <p>Hello {UserName},</p>
                        <p>Your One-Time Password (OTP) for verification is: <strong>{OtpCode}</strong></p>
                        <p><strong>Request Details:</strong></p>
                        <ul>
                            <li><strong>IP Address:</strong> {Ip}</li>
                            <li><strong>Location:</strong> {Country}</li>
                            <li><strong>Browser and OS:</strong> {UserAgent}</li>
                        </ul>
                        <p>If you did not request this code, please ignore this email or contact our support team.</p>
                        <p>&copy; 2025 AbreuHD. All rights reserved.</p>
                    </body>
                    </html>";
        }
    }
}
