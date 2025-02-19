using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace Auth.Infraestructure.Identity.Features.UserSessions.Commands
{
    public class LogoutCurrentSessionCommand : IRequest<GenericApiResponse<bool>>
    {
        public required string Token { get; set; }
    }
    internal class LogoutCurrentSessionCommandHandler(IdentityContext identityContext) : IRequestHandler<LogoutCurrentSessionCommand, GenericApiResponse<bool>>
    {
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<bool>> Handle(LogoutCurrentSessionCommand request, CancellationToken cancellationToken)
        {
            var identityResponse = await _identityContext.Set<Entities.UserSession>().FirstOrDefaultAsync(x => x.Token == request.Token, cancellationToken);
            if(identityResponse == null)
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
