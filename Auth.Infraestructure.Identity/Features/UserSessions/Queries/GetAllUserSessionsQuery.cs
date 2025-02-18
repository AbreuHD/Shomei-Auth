using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Sessions;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infraestructure.Identity.Features.UserSessions.Queries
{
    public class GetAllUserSessionsQuery : IRequest<GenericApiResponse<List<UserSessionResponse>>>
    {
        public required string UserId { get; set; }
    }
    public class GetAllUserSessionsQueryHandler(IdentityContext identityContext) : IRequestHandler<GetAllUserSessionsQuery, GenericApiResponse<List<UserSessionResponse>>>
    {
        private readonly IdentityContext _identityContext = identityContext;

        public async Task<GenericApiResponse<List<UserSessionResponse>>> Handle(GetAllUserSessionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userSessions = await _identityContext.Set<UserSession>()
                    .Where(x => x.UserId == request.UserId)
                    .Select(x => new UserSessionResponse
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Token = x.Token,
                        IpAddress = x.IpAddress,
                        UserAgent = x.UserAgent,
                        CreatedAt = x.CreatedAt,
                    })
                    .ToListAsync(cancellationToken);
                return new GenericApiResponse<List<UserSessionResponse>>
                {
                    Message = "User sessions retrieved successfully",
                    Statuscode = StatusCodes.Status200OK,
                    Success = true,
                    Payload = userSessions
                };
            }
            catch (Exception e)
            {
                return new GenericApiResponse<List<UserSessionResponse>>
                {
                    Message = e.Message,
                    Statuscode = StatusCodes.Status500InternalServerError,
                    Success = false,
                    Payload = []
                };

            }
        }
    }
}
