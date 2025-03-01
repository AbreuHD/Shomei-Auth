﻿using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Auth.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmail
{
    /// <summary>
    /// This class is used to authenticate and confirm a user's email by validating a confirmation token.
    /// It handles the process of confirming the user's email address during the account activation process.
    /// </summary>
    public class AuthEmailCommand : IRequest<GenericApiResponse<string>>
    {
        /// <summary>
        /// The data transfer object (DTO) that contains the necessary information for email confirmation.
        /// It includes the user ID and the confirmation token that will be validated to authenticate and confirm the email.
        /// </summary>
        /// <value>
        /// A <see cref="ConfirmEmailRequestDto"/> object that holds the user ID and the token needed for the email confirmation process.
        /// </value>
        public required ConfirmEmailRequestDto Dto { get; set; }
    }

    internal class AuthEmailCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<AuthEmailCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<GenericApiResponse<string>> Handle(AuthEmailCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<string>()
            {
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = "N/A"
            };
            try
            {
                var user = await _userManager.FindByIdAsync(request.Dto.UserId);
                if (user == null)
                {
                    response.Message = $"Not account registered with this user";
                    response.Statuscode = StatusCodes.Status404NotFound;
                    response.Payload = "N/A";
                    response.Success = false;
                    return response;
                }

                request.Dto.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Dto.Token));

                var result = await _userManager.ConfirmEmailAsync(user, request.Dto.Token);

                if (!result.Succeeded)
                {
                    response.Message = $"An error occurred while confirming {user.Email}";
                    response.Statuscode = StatusCodes.Status400BadRequest;
                    response.Payload = result.Errors.FirstOrDefault()!.Description ?? "ERROR";
                    response.Success = false;
                    return response;
                }

                response.Success = true;
                response.Message = $"Account confirmed for {user.Email}. You can now use the App";
                response.Statuscode = StatusCodes.Status200OK;
                response.Payload = "OK";
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred while confirming the account";
                response.Statuscode = StatusCodes.Status500InternalServerError;
                response.Payload = ex.Message;
                response.Success = false;
            }
            return response;
        }
    }
}
