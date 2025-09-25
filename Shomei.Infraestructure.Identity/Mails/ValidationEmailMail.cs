namespace Shomei.Infraestructure.Identity.Mails
{
    public static class ValidationEmailMail
    {
        public static string GetEmailHtml(string activationLink)
        {
            return $@"<!DOCTYPE html>
                            <html lang='en'>
                            <head>
                                <meta charset='UTF-8'>
                                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                <title>Account Activation</title>
                            </head>
                            <body>
                                <h2>Activate Your Account</h2>
                                <p>Hello,</p>
                                <p>Thank you for registering. Please activate your account by clicking the button below:</p>
                                <p><a href='{activationLink}' style='display: inline-block; padding: 10px 20px; color: #fff; background-color: #007bff; text-decoration: none; border-radius: 5px;'>Activate Account</a></p>
                                <p>If you did not register, please ignore this email.</p>
                                <p>&copy; 2025 AbreuHD. All rights reserved.</p>
                            </body>
                            </html>";
        }
    }
}
