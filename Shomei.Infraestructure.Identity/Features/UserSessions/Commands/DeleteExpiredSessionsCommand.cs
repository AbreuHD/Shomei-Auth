using MediatR;
using Microsoft.EntityFrameworkCore;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.Entities;

namespace Shomei.Infraestructure.Identity.Features.UserSessions.Commands
{
    /// <summary>
    /// Command to delete expired user sessions from the database.
    /// </summary>
    /// <remarks>
    /// This command is used to find and delete sessions that have expired, ensuring that inactive sessions are removed from the system.
    /// </remarks>
    public class DeleteExpiredSessionsCommand : IRequest<Unit> { }

    internal class DeleteExpiredSessionsCommandHandler(IdentityContext _identityContext)
        : IRequestHandler<DeleteExpiredSessionsCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteExpiredSessionsCommand request, CancellationToken cancellationToken)
        {
            var expiredSessions = await _identityContext.Set<UserSession>()
                .Where(s => s.Expiration < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            if (expiredSessions.Count != 0)
            {
                _identityContext.RemoveRange(expiredSessions);
                await _identityContext.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
