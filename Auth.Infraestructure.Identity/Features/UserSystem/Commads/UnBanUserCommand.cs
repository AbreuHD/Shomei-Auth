using MediatR;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.DTOs.Users;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Services;

namespace Shomei.Infraestructure.Identity.Features.UserSystem.Commads
{
    public class UnBanUserCommand : IRequest<GenericApiResponse<bool>>
    {
        public required BanUnbanUserRequestDto Dto { get; set; }
    }
    internal class UnBanUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<UnBanUserCommand, GenericApiResponse<bool>>
    {
        private readonly BanService _banService = new(userManager);

        public async Task<GenericApiResponse<bool>> Handle(UnBanUserCommand request, CancellationToken cancellationToken)
        {
            return await _banService.UnbanUserAsync(request.Dto.UserId);
        }
    }
}
