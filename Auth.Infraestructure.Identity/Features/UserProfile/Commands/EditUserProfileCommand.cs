using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Commands
{
    public class EditUserProfileCommand : IRequest<GenericApiResponse<bool>>
    {
        public required int Id { get; set; }
        [JsonIgnore]
        public string? UserId { get; set; }
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class EditUserProfileCommandHandler(IdentityContext context) : IRequestHandler<EditUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;

        public async Task<GenericApiResponse<bool>> Handle(EditUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var UserProfile = await _context.Set<Entities.UserProfile>().FindAsync([request.Id], cancellationToken: cancellationToken)
                    ?? throw new NotImplementedException("This profile don't exist");

                if (UserProfile.UserId != request.UserId)
                {
                    return new GenericApiResponse<bool> { Success = false, Message = "You User don't have permission to do that", Statuscode = StatusCodes.Status401Unauthorized, Payload = false };
                }

                UserProfile.AvatarUrl = request.AvatarUrl;
                UserProfile.Name = request.Name;
                _context.Set<Entities.UserProfile>().Update(UserProfile);
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
