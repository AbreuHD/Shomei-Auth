using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Email;
using Auth.Infraestructure.Identity.DTOs.Geolocation;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Infraestructure.Identity.Extra
{
    public static class ExtraMethods
    {
        public static async Task<bool> SendEmail(MailSettings mailSettings, SendEmailRequestDto requestDto)
        {
            try
            {
                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse($"{mailSettings.DisplayName} <{mailSettings.EmailFrom}>")
                };
                email.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.EmailFrom));
                email.To.Add(MailboxAddress.Parse(requestDto.To));
                email.Subject = requestDto.Subject;
                BodyBuilder bodyBuilder = new()
                {
                    HtmlBody = requestDto.Body
                };
                email.Body = bodyBuilder.ToMessageBody();

                using SmtpClient smtp = new();
                smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    return sslPolicyErrors == SslPolicyErrors.None;
                };
                await smtp.ConnectAsync(mailSettings.SmtpHost, mailSettings.SmtpPort, SecureSocketOptions.SslOnConnect);
                await smtp.AuthenticateAsync(mailSettings.SmtpUser, mailSettings.SmtpPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<GeoLocationInfoDto?> GetGeoLocationInfo(string ipAddress, IHttpClientFactory _httpClientFactory)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync($"https://ip.guide/{ipAddress}");

                if (!string.IsNullOrEmpty(response))
                {
                    var geoInfo = JsonConvert.DeserializeObject<LocationInfoDto>(response);
                    return geoInfo?.Location;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener información de geolocalización: {ex.Message}");
            }

            return null;
        }

        internal static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        internal static string RandomTokenString()
        {
            var randomBytes = new byte[40];
            RandomNumberGenerator.Fill(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        internal static RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
        }

        internal static async Task<string> SendVerificationEMailUrl(ApplicationUser user, string origin, UserManager<ApplicationUser> _userManager)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "Account/EmailConfirm";
            var url = new Uri(string.Concat($"{origin}/", route));
            var verificationUrl = QueryHelpers.AddQueryString(url.ToString(), "userId", user.Id);
            verificationUrl = QueryHelpers.AddQueryString(verificationUrl, "token", code);

            return verificationUrl;
        }

        internal static JwtSecurityToken GenerateJWToken(ApplicationUser user, JwtSettings _jwtSettings, IList<Claim> userClaims, IList<string> roles, int? profileId)
        {
            var roleClaims = new List<Claim>();
            foreach (var role in roles)
            {
                roleClaims.Add(new Claim("roles", role));
            }

            var claims = new List<Claim>
                   {
                       new(JwtRegisteredClaimNames.Sub, user.UserName!),
                       new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                       new(JwtRegisteredClaimNames.Email, user.Email!),
                       new("uid", user.Id),
                   }
            .Union(userClaims)
            .Union(roleClaims)
            .ToList();

            if (profileId.HasValue)
                claims.Add(new Claim("ProfileId", profileId.Value.ToString()));

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken
            (
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signCredentials
            );

            return jwtSecurityToken;
        }
    }
}
