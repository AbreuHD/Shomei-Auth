using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Email;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Enums;
using Auth.Infraestructure.Identity.Otp;
using Auth.Infraestructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    public class ChangeEmailWithOtpCommand : IRequest<GenericApiResponse<bool>>
    {
        public required ChangeEmailWithOtpRequestDto Dto { get; set; }
        public required string UserId { get; set; }
    }
    internal class ChangeEmailWithOtpCommandHandler(UserManager<ApplicationUser> userManager,
            IdentityContext identityContext) : IRequestHandler<ChangeEmailWithOtpCommand, GenericApiResponse<bool>>
    {
        private readonly EmailChangeService _emailChangeService = new(userManager,identityContext);

        public async Task<GenericApiResponse<bool>> Handle(ChangeEmailWithOtpCommand request, CancellationToken cancellationToken)
        {
            return await _emailChangeService.ChangeEmailAsync(request.UserId, request.Dto.NewEmail, otp: request.Dto.Otp);
        }
    }
}