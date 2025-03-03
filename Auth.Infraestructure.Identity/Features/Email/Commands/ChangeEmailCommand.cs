using Auth.Infraestructure.Identity.DTOs.Email;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.Email.Commands
{
    public class ChangeEmailCommand : IRequest<GenericApiResponse<bool>>
    {
        public required ChangeEmailRequestDto Dto { get; set; }
        public required string UserId { get; set; }
    }
    internal class ChangeEmailCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ChangeEmailCommand, GenericApiResponse<bool>>
    {
        private readonly EmailChangeService _emailChangeService = new(userManager);

        public async Task<GenericApiResponse<bool>> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
        {
            return await _emailChangeService.ChangeEmailAsync(request.UserId, request.Dto.NewEmail, request.Dto.Password);
        }
    }
}