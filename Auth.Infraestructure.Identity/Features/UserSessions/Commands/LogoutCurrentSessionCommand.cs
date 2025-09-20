using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Extra;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;


namespace Auth.Infraestructure.Identity.Features.UserSessions.Commands
{
    /// <summary>
    /// Command to log out the current session using the provided token.
    /// </summary>
    /// <remarks>
    /// This command finds the session associated with the given token and removes it from the database,
    /// effectively logging the user out of the current session.
    /// </remarks>
    public class LogoutCurrentSessionCommand : IRequest<GenericApiResponse<bool>>
    {
        /// <summary>
        /// The token of the session to be logged out.
        /// </summary>
        public required string Token { get; set; }
    }
    internal class LogoutCurrentSessionCommandHandler(IdentityContext identityContext) : IRequestHandler<LogoutCurrentSessionCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> Handle(LogoutCurrentSessionCommand request, CancellationToken cancellationToken)
        {
            var hashedToken = ExtraMethods.GetHash(request.Token);
            var identityResponse = await _identityContext.Set<Entities.UserSession>().FirstOrDefaultAsync(x => x.Token == hashedToken, cancellationToken);
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
