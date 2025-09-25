using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Services;

namespace Shomei.Infraestructure.Identity.Features.Email.Commands
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