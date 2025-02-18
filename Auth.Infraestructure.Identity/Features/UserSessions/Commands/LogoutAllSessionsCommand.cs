using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.Features.UserSessions.Commands
{
    class LogoutAllSessionsCommand : IRequest<GenericApiResponse<bool>>
    {
        public required string UserId { get; set; }
    }
    sealed class LogoutAllSessionsCommandHandler(IdentityContext identityContext) : IRequestHandler<LogoutAllSessionsCommand, GenericApiResponse<bool>>
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
