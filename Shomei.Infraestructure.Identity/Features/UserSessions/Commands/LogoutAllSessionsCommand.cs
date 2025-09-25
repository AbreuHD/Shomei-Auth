using MediatR;
using Microsoft.AspNetCore.Http;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Entities;

namespace Shomei.Infraestructure.Identity.Features.UserSessions.Commands
{
    /// <summary>
    /// Command to log out all sessions for a specific user.
    /// </summary>
    /// <remarks>
    /// This command removes all active sessions for the given user from the database,
    /// effectively logging the user out of all devices or sessions.
    /// </remarks>
    public class LogoutAllSessionsCommand : IRequest<GenericApiResponse<bool>>
    {
    }
    internal class LogoutAllSessionsCommandHandler(IdentityContext identityContext, IHttpContextAccessor httpContextAccessor) : IRequestHandler<LogoutAllSessionsCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<GenericApiResponse<bool>> Handle(LogoutAllSessionsCommand request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext.User.FindFirst("uid").Value;
            var identityResponse = _identityContext.Set<UserSession>().Where(x => x.UserId == UserId);
            if (identityResponse == null)
            {
                return new GenericApiResponse<bool>
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Sessions not found",
                    Payload = false
                };
            }

            _identityContext.Set<UserSession>().RemoveRange(identityResponse);
            await _identityContext.SaveChangesAsync(cancellationToken);

            return new GenericApiResponse<bool>
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "All Sessions Logout Sucessfully",
                Payload = true
            };
        }
    }
}
