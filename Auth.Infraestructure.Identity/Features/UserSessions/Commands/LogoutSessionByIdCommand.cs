using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Auth.Infraestructure.Identity.Features.UserSessions.Commands
{
    /// <summary>
    /// Command to log out a session by its ID.
    /// </summary>
    /// <remarks>
    /// This command finds the session associated with the given ID and removes it from the database,
    /// effectively logging the user out of the specified session.
    /// </remarks>
    public class LogoutSessionByIdCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// The ID of the session to be logged out.
        /// </summary>
        public required int Id { get; set; }
    }
    internal class LogoutSessionByIdCommandHandler(IdentityContext identityContext) : IRequestHandler<LogoutSessionByIdCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> Handle(LogoutSessionByIdCommand request, CancellationToken cancellationToken)
        {
            var identityResponse = await _identityContext.Set<Entities.UserSession>().FindAsync([request.Id], cancellationToken: cancellationToken);
            if (identityResponse == null)
            {
                return new GenericApiResponse<bool>
                {
                    Success = false,
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "Session not found",
                    Payload = false
                };
            }

            _identityContext.Set<Entities.UserSession>().Remove(identityResponse);
            await _identityContext.SaveChangesAsync(cancellationToken);

            return new GenericApiResponse<bool>
            {
                Success = true,
                Statuscode = StatusCodes.Status200OK,
                Message = "Logout Sucessfully",
                Payload = true
            };
        }
    }
}
