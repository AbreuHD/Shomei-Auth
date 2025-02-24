using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Commands
{
    /// <summary>
    /// Command to edit a user profile.
    /// </summary>
    /// <remarks>
    /// This command allows updating a user's profile, including the user's name and avatar URL.
    /// Only the user with the matching UserId is allowed to make the changes.
    /// </remarks>
    public class EditUserProfileCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// The profile data that needs to be updated.
        /// </summary>
        /// <see cref="EditUserProfileRequestDto"/>
        /// <remarks>
        /// This object holds the updated values for the user's profile, such as their name and avatar URL.
        /// </remarks>
        public required EditUserProfileRequestDto Dto { get; set; }

        /// <summary>
        /// The ID of the user whose profile is being edited.
        /// </summary>
        /// <remarks>
        /// This value must match the UserId of the profile being edited to ensure the correct user is making the request.
        /// </remarks>
        public required string UserId { get; set; }
    }

    internal class EditUserProfileCommandHandler(IdentityContext context) : IRequestHandler<EditUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;

        public async Task<GenericApiResponse<bool>> Handle(EditUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var UserProfile = await _context.Set<Entities.UserProfile>().FindAsync([request.Dto.Id], cancellationToken: cancellationToken)
                    ?? throw new NotImplementedException("This profile don't exist");

                if (UserProfile.UserId != request.UserId)
                {
                    return new GenericApiResponse<bool> { Success = false, Message = "You User don't have permission to do that", Statuscode = StatusCodes.Status401Unauthorized, Payload = false };
                }

                UserProfile.AvatarUrl = request.Dto.AvatarUrl;
                UserProfile.Name = request.Dto.Name;
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
