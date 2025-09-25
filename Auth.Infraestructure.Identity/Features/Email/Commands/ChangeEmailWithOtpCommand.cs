using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    public class ChangeEmailWithOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        public required string NewEmail { get; set; }
        public required string Otp { get; set; }
    }
    internal class ChangeEmailWithOtpCommandHandler(UserManager<ApplicationUser> userManager,
            IdentityContext identityContext, IHttpContextAccessor httpContextAccessor) : IRequestHandler<ChangeEmailWithOtpCommand, GenericApiResponse<bool>>
    {
        private readonly EmailChangeService _emailChangeService = new(userManager, identityContext);
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<bool>> Handle(ChangeEmailWithOtpCommand request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext?.User.FindFirst("uid")?.Value ?? "Unknown";
            return await _emailChangeService.ChangeEmailAsync(UserId, request.NewEmail, otp: request.Otp);
        }
    }
}