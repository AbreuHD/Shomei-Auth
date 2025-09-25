using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Extra;
using System.ComponentModel.DataAnnotations;

namespace Shomei.Infraestructure.Identity.Features.UserProfile.Commands
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
        /// The full name of the user.
        /// </summary>
        /// <remarks>
        /// The user's name is a required field and will be used to identify the user on the platform.
        /// </remarks>
        public required string Name { get; set; }

        /// <summary>
        /// The URL to the user's avatar image.
        /// </summary>
        /// <remarks>
        /// This field is optional. If provided, it will be used to display the user's profile picture. 
        /// If not provided, the user will have a default avatar.
        /// </remarks>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// The password for the user profile.
        /// </summary>
        /// <remarks>
        /// This field is optional. If provided, it can be used to set or update the user's password.
        /// If not provided, the profile not use password.
        /// </remarks>
        [RegularExpression(@"^\d{4}$", ErrorMessage = "La contraseña debe tener exactamente 4 dígitos numéricos.")]
        public string? Password { get; set; }
    }
    internal class CreateUserProfileCommandHandler(IdentityContext context, IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<bool>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var uidClaim = user?.FindFirst("uid")?.Value;
            if (uidClaim == null)
            {
                return new GenericApiResponse<bool> { Success = false, Message = "You are not logged", Statuscode = StatusCodes.Status400BadRequest, Payload = false };
            }
            try
            {
                var existingProfile = await _context.Set<Entities.UserProfile>()
                    .FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
                if (existingProfile != null)
                {
                    return new GenericApiResponse<bool> { Success = false, Message = "User Already Exists", Statuscode = StatusCodes.Status400BadRequest, Payload = false };
                }
                if (string.IsNullOrEmpty(request.Password))
                {
                    request.Password = null;
                }
                else
                {
                    request.Password = ExtraMethods.GetHash(request.Password);
                }
                var profile = new Entities.UserProfile { UserId = uidClaim, Name = request.Name, AvatarUrl = request.AvatarUrl, Password = request.Password };
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
