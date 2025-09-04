using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Commands
{
    /// <summary>
    /// Command to delete a user profile.
    /// </summary>
    /// <remarks>
    /// This command is used to delete a user profile from the database. It verifies if the user has the correct permissions
    /// before performing the deletion.
    /// </remarks>
    public class DeleteUserProfileCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// The ID of the user profile to delete.
        /// </summary>
        /// <remarks>
        /// The ID is used to locate the specific user profile in the database.
        /// </remarks>
        public required int Id { get; set; }

        /// <summary>
        /// The ID of the user attempting to delete the profile.
        /// </summary>
        /// <remarks>
        /// The UserId must match the owner of the profile being deleted. This is used for authorization checks.
        /// </remarks>
        public required string UserId { get; set; }
    }
    public class DeleteUserProfileCommandHandler(IdentityContext context) : IRequestHandler<DeleteUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;

        public async Task<GenericApiResponse<bool>> Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userProfile = await _context.Set<Entities.UserProfile>().FindAsync([request.Id], cancellationToken: cancellationToken);

            if (userProfile is null)
            {
                return new GenericApiResponse<bool> { Message = "User profile don't exist", Payload = false, Statuscode = StatusCodes.Status500InternalServerError, Success = false };
            }
            if (userProfile.UserId != request.UserId)
            {
                return new GenericApiResponse<bool> { Message = "You don't have permission to do that", Payload = false, Statuscode = StatusCodes.Status401Unauthorized, Success = false };
            }
            _context.Set<Entities.UserProfile>().Remove(userProfile);
            await _context.SaveChangesAsync();
            return new GenericApiResponse<bool> { Message = "Deleted", Payload = true, Statuscode = StatusCodes.Status200OK, Success = true };

        }
    }
}
