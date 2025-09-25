using MediatR;
using Microsoft.AspNetCore.Http;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;

namespace Shomei.Infraestructure.Identity.Features.UserProfile.Commands
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
        /// The unique identifier for the user profile being updated.
        /// </summary>
        /// <remarks>
        /// This ID is used to locate the specific user profile that needs to be edited.
        /// </remarks>
        public required int Id { get; set; }

        /// <summary>
        /// The URL of the user's avatar image.
        /// </summary>
        /// <remarks>
        /// This field is optional, and it will update the user's avatar URL if provided.
        /// </remarks>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// The name of the user, typically their full name.
        /// </summary>
        /// <remarks>
        /// This field is required and will update the user's name.
        /// </remarks>
        public required string Name { get; set; }
    }

    internal class EditUserProfileCommandHandler(IdentityContext context, IHttpContextAccessor httpContextAccessor) : IRequestHandler<EditUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<bool>> Handle(EditUserProfileCommand request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext.User.FindFirst("uid").Value;
            try
            {
                var UserProfile = await _context.Set<Entities.UserProfile>().FindAsync([request.Id], cancellationToken: cancellationToken)
                    ?? throw new NotImplementedException("This profile don't exist");

                if (UserProfile.UserId != UserId)
                {
                    return new GenericApiResponse<bool> { Success = false, Message = "You User don't have permission to do that", Statuscode = StatusCodes.Status401Unauthorized, Payload = false };
                }

                UserProfile.AvatarUrl = request.AvatarUrl;
                UserProfile.Name = request.Name;
                _context.Set<Entities.UserProfile>().Update(UserProfile);
                await _context.SaveChangesAsync(cancellationToken);

                return new GenericApiResponse<bool> { Success = true, Message = "Profile Edited Sucessfully", Statuscode = StatusCodes.Status200OK, Payload = true };
            }
            catch (Exception e)
            {
                return new GenericApiResponse<bool> { Success = false, Message = e.Message, Statuscode = StatusCodes.Status404NotFound, Payload = false };
            }
        }
    }
}
