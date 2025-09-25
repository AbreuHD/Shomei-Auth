using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Extra;


namespace Shomei.Infraestructure.Identity.Features.UserSessions.Commands
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
    }
    internal class LogoutCurrentSessionCommandHandler(IdentityContext identityContext, IHttpContextAccessor httpContextAccessor) : IRequestHandler<LogoutCurrentSessionCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<bool>> Handle(LogoutCurrentSessionCommand request, CancellationToken cancellationToken)
        {
            var Token = _httpContextAccessor.HttpContext.Request.Headers.Authorization.ToString().Split(" ")[1];
            var hashedToken = ExtraMethods.GetHash(Token);
            var identityResponse = await _identityContext.Set<UserSession>().FirstOrDefaultAsync(x => x.Token == hashedToken, cancellationToken);
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

            _identityContext.Set<UserSession>().Remove(identityResponse);
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
