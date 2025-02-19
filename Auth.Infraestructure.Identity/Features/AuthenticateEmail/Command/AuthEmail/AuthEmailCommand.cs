using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.PublicDtos;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Auth.Infraestructure.Identity.Features.AuthenticateEmail.Command.AuthEmail
{
    public class AuthEmailCommand : IRequest<GenericApiResponse<string>>
    {
        public required ConfirmEmailRequestDto Dto { get; set; }
    }

    public class AuthEmailCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<AuthEmailCommand, GenericApiResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<GenericApiResponse<string>> Handle(AuthEmailCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<string>();
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
