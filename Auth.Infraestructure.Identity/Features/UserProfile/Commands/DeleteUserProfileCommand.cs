using Auth.Core.Application.DTOs.Generic;
using Auth.Infraestructure.Identity.Context;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Commands
{
    public class DeleteUserProfileCommand : IRequest<GenericApiResponse<bool>>
    {
        public required int Id { get; set; }
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
            return new GenericApiResponse<bool> { Message = "Deleted", Payload = false, Statuscode = StatusCodes.Status200OK, Success = false };

        }
    }
}
