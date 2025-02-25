using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Account;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Commands
{
    /// <summary>
    /// Command to create a user profile in the system.
    /// </summary>
    /// <remarks>
    /// This command is used to handle the creation of a user profile, which includes adding details such as the user's name and avatar URL. 
    /// It requires a User ID to associate the profile with a specific user.
    /// </remarks>
    public class CreateUserProfileCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// The user profile data transfer object (DTO) containing user profile details.
        /// </summary>
        /// <remarks>
        /// This object includes necessary fields like the user's name and avatar URL. 
        /// It is passed to the handler for creating a new profile in the database.
        /// </remarks>
        public required CreateUserProfileRequestDto Dto { get; set; }

        /// <summary>
        /// The ID of the user for whom the profile is being created.
        /// </summary>
        /// <remarks>
        /// The User ID is used to link the profile to an existing user in the system. 
        /// It ensures that the profile is associated with the correct user.
        /// </remarks>
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
