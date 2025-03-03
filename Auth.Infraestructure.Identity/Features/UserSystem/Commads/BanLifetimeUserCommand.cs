using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Users;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.UserSystem.Commads
{
    public class BanLifetimeUserCommand : IRequest<GenericApiResponse<bool>>
    {
        public required BanUnbanUserRequestDto Dto { get; set; }
    }
    internal class BanLifetimeUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<BanLifetimeUserCommand, GenericApiResponse<bool>>
    {
        private readonly BanService _banService = new(userManager);

        public async Task<GenericApiResponse<bool>> Handle(BanLifetimeUserCommand request, CancellationToken cancellationToken)
        {
            return await _banService.BanUserAsync(request.Dto.UserId);
        }
    }
}
