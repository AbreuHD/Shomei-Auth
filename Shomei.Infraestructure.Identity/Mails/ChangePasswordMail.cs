namespace Shomei.Infraestructure.Identity.Mails
{
    public static class ChangePasswordMail
    {
        public static string GetEmailHtml(string Ip, string Country, string UserAgent)
        {
            return $@"<!DOCTYPE html>
                                    <html lang='en'>
                                    <head>
                                        <meta charset='UTF-8'>
                                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                        <title>Password Change Notification</title>
                                    </head>
                                    <body>
                                        <h2>Password Change Notification</h2>
                                        <p>Hello,</p>
                                        <p>Your password has been successfully changed.</p>
                                        <p><strong>Change Details:</strong></p>
                                        <ul>
                                            <li><strong>IP Address:</strong> {Ip}</li>
                                            <li><strong>Location:</strong> {Country}</li>
                                            <li><strong>Browser and OS:</strong> {UserAgent}</li>
                                        </ul>
                                        <p>If you did not make this change, please contact our support team immediately.</p>
                                        <p>&copy; 2025 AbreuHD. All rights reserved.</p>
                                    </body>
                                    </html>";
        }
    }
}