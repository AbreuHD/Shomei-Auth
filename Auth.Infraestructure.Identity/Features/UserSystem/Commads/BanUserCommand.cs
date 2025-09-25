using MediatR;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.DTOs.Users;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Services;

namespace Shomei.Infraestructure.Identity.Features.UserSystem.Commads
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
