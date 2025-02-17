using Auth.Core.Application.DTOs.Generic;
using Auth.Infraestructure.Identity.Context;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json.Serialization;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Commands
{
    public class CreateUserProfileCommand : IRequest<GenericApiResponse<bool>>
    {
        [JsonIgnore]
        public string? UserId { get; set; }
        public required string Name { get; set; }
        public string? AvatarUrl { get; set; }
    }
    public class CreateUserProfileCommandHandler(IdentityContext context) : IRequestHandler<CreateUserProfileCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _context = context;

        public async Task<GenericApiResponse<bool>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var profile = new Entities.UserProfile { UserId = request.UserId, Name = request.Name, AvatarUrl = request.AvatarUrl };
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
