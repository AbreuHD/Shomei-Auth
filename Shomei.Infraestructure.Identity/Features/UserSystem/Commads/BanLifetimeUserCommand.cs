using MediatR;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.DTOs.Users;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Services;

namespace Shomei.Infraestructure.Identity.Features.UserSystem.Commads
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
