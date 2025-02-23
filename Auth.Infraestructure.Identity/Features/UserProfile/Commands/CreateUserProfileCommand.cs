using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Commands
{
    public class CreateUserProfileCommand : IRequest<GenericApiResponse<bool>>
    {
        public required CreateUserProfileRequestDto Dto { get; set; }
        public required string UserId { get; set; }
    }
    internal class CreateUserProfileCommandHandler(IdentityContext context) : IRequestHandler<CreateUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;

        public async Task<GenericApiResponse<bool>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var profile = new Entities.UserProfile { UserId = request.UserId, Name = request.Dto.Name, AvatarUrl = request.Dto.AvatarUrl };
                await _context.Set<Entities.UserProfile>().AddAsync(profile, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return new GenericApiResponse<bool> { Success = true, Message = "Profile Added Sucessfully", Statuscode = StatusCodes.Status200OK, Payload = true };
            }
            catch (Exception e)
            {
                return new GenericApiResponse<bool> { Success = false, Message = e.Message, Statuscode = StatusCodes.Status404NotFound, Payload = false };
            }
        }
    }
}
