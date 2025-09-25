using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    /// <summary>
    /// Represents a command to change a user's email address.
    /// This command requires the user's current password to validate the request.
    /// </summary>
    public class ChangeEmailCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// Gets or sets the new email address that the user wants to set.
        /// The email must be valid and unique in the system.
        /// </summary>
        public required string NewEmail { get; set; }
        /// <summary>
        /// Gets or sets the user's current password.
        /// This is required to confirm the user's identity before changing the email.
        /// </summary>
        public required string Password { get; set; }
    }
    internal class ChangeEmailCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor) : IRequestHandler<ChangeEmailCommand, GenericApiResponse<bool>>
    {
        private readonly EmailChangeService _emailChangeService = new(userManager);
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<bool>> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext?.User.FindFirst("uid")?.Value ?? "Unknown";
            return await _emailChangeService.ChangeEmailAsync(UserId, request.NewEmail, request.Password);
        }
    }
}