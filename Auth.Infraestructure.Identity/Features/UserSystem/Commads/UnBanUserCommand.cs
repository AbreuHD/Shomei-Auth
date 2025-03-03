using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Users;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Features.UserSystem.Commads
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
