using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Migrations;
using Auth.Infraestructure.Identity.Otp;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Auth.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmailWithOtp
{
    /// <summary>
    /// Command used to confirm a user's email by validating a One-Time Password (OTP).
    /// </summary>
    public class AuthEmailWithOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// The OTP code sent to the user that must be validated to confirm the email.
        /// </summary>
        public required string Otp { get; set; }
        /// <summary>
        /// The unique identifier of the user whose email is being confirmed.
        /// </summary>
        /// <value>
        /// A string representing the user ID, which is required for email confirmation.
        /// </value>
        public required string UserId { get; set; }
    }

    internal class AuthEmailWithOtpCommandHandler(UserManager<ApplicationUser> userManager, IdentityContext identityContext) : IRequestHandler<AuthEmailWithOtpCommand, GenericApiResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IdentityContext _identityContext = identityContext;
        public async Task<GenericApiResponse<bool>> Handle(AuthEmailWithOtpCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<bool>()
            {
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = "N/A"
            };
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    response.Message = $"Not account registered with this user";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    response.Payload = false;
                    response.Success = false;
                    return response;
                }

                var otpCheckResponse = await ValidateOtpEmail.ValidateOtpWithEmail(response, request.Otp, user.Id, OtpPurpose.VerifyAccount.ToString(), _identityContext);
                if (!otpCheckResponse.Success)
                {
                    return otpCheckResponse;
                }

                var Token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var result = await _userManager.ConfirmEmailAsync(user, Token);
                if (!result.Succeeded)
                {
                    response.Message = $"An error occurred while confirming {user.Email}";
                    response.Statuscode = StatusCodes.Status400BadRequest;
                    response.Payload = false;
                    response.Success = false;
                    return response;
                }

                response.Success = true;
                response.Message = $"Account confirmed for {user.Email}. You can now use the App";
                response.Statuscode = StatusCodes.Status200OK;
                response.Payload = true;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred while confirming the account {ex.Message}";
                response.Statuscode = StatusCodes.Status500InternalServerError;
                response.Payload = false;
                response.Success = false;
            }
            return response;
        }
    }
}
