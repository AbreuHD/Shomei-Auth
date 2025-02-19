using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infraestructure.Identity.Features.UserSessions.Commands
{
    public class LogoutSessionByIdCommand : IRequest<GenericApiResponse<bool>>
    {
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
