using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Auth.Infraestructure.Identity.Features.UserSessions.Commands
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
        /// <summary>
        /// The unique identifier of the user whose sessions will be logged out.
        /// </summary>
        public required string UserId { get; set; }
    }
    internal class LogoutAllSessionsCommandHandler(IdentityContext identityContext) : IRequestHandler<LogoutAllSessionsCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> Handle(LogoutAllSessionsCommand request, CancellationToken cancellationToken)
        {
            var identityResponse = _identityContext.Set<Entities.UserSession>().Where(x => x.UserId == request.UserId);
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

            _identityContext.Set<Entities.UserSession>().RemoveRange(identityResponse);
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
