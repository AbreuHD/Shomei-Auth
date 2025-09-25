using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Entities;

namespace Shomei.Infraestructure.Identity.Features.UserProfile.Commands
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
        /// Gets or sets the user's current password.
        /// </summary>
        /// <remarks>
        /// This is required to validate the identity of the user before deleting the profile.
        /// </remarks>
        public required string Password { get; set; }
    }
    public class DeleteUserProfileCommandHandler(IdentityContext context, IHttpContextAccessor httpContextAccessor, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : IRequestHandler<DeleteUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        public async Task<GenericApiResponse<bool>> Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext.User.FindFirst("uid")?.Value ?? "Unknown";
            var userProfile = await _context.Set<Entities.UserProfile>().FindAsync([request.Id], cancellationToken: cancellationToken);

            var User = await _userManager.FindByIdAsync(UserId);


            if (userProfile is null)
            {
                return new GenericApiResponse<bool> { Message = "User profile don't exist", Payload = false, Statuscode = StatusCodes.Status500InternalServerError, Success = false };
            }
            if (userProfile.UserId != UserId)
            {
                return new GenericApiResponse<bool> { Message = "You don't have permission to do that", Payload = false, Statuscode = StatusCodes.Status401Unauthorized, Success = false };
            }
            var signInResult = await _signInManager.CheckPasswordSignInAsync(User, request.Password, false);
            if (!signInResult.Succeeded)
            {
                return new GenericApiResponse<bool> { Message = "Invalid password", Payload = false, Statuscode = StatusCodes.Status401Unauthorized, Success = false };
            }
            _context.Set<Entities.UserProfile>().Remove(userProfile);
            await _context.SaveChangesAsync(cancellationToken);
            return new GenericApiResponse<bool> { Message = "Deleted", Payload = true, Statuscode = StatusCodes.Status200OK, Success = true };

        }
    }
}
