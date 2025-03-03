using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Users;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.UserSystem.Commads
{
    public class BanUserCommand : IRequest<GenericApiResponse<bool>>
    {
        public required BanUserRequestDto Dto { get; set; }
    }
    internal class BanUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<BanUserCommand, GenericApiResponse<bool>>
    {
        private readonly BanService _banService = new(userManager);

        public async Task<GenericApiResponse<bool>> Handle(BanUserCommand request, CancellationToken cancellationToken)
        {
            return await _banService.BanUserAsync(request.Dto.UserId, request.Dto.Duration);
        }
    }
}
