using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Enums;
using Shomei.Infraestructure.Identity.Otp;

namespace Shomei.Infraestructure.Identity.Services
{
    internal class EmailChangeService(UserManager<ApplicationUser> userManager, IdentityContext? identityContext = null)
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IdentityContext? _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> ChangeEmailAsync(string userId, string newEmail, string? password = null, string? otp = null)
        {
            var response = new GenericApiResponse<bool>
            {
                Payload = false,
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = ""
            };

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    response.Message = "User not found.";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    return response;
                }

                var emailExists = await _userManager.FindByEmailAsync(newEmail);
                if (emailExists != null)
                {
                    response.Message = $"Email {newEmail} is already taken.";
                    response.Statuscode = StatusCodes.Status406NotAcceptable;
                    return response;
                }

                if (!string.IsNullOrEmpty(otp))
                {
                    if (_identityContext == null)
                    {
                        response.Message = "OTP validation requires IdentityContext.";
                        return response;
                    }

                    var otpCheckResponse = await ValidateOtpEmail.ValidateOtpWithEmail(response, otp, user.Id, OtpPurpose.ChangeEmail.ToString(), _identityContext);
                    if (!otpCheckResponse.Success)
                    {
                        return otpCheckResponse;
                    }
                }

                if (!string.IsNullOrEmpty(password))
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, password);
                    if (!passwordCheck)
                    {
                        response.Message = "Password is incorrect.";
                        response.Statuscode = StatusCodes.Status406NotAcceptable;
                        return response;
                    }
                }

                var result = await _userManager.SetEmailAsync(user, newEmail);
                if (result.Succeeded)
                {
                    response.Payload = true;
                    response.Success = true;
                    response.Statuscode = StatusCodes.Status200OK;
                    response.Message = "Email changed successfully.";
                }
                else
                {
                    response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
