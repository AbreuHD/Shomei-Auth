using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.Features.UserSessions.Commands
{
    public class DeleteExpiredSessionsCommand : IRequest<Unit> { }

    public class DeleteExpiredSessionsCommandHandler(IdentityContext _identityContext)
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
