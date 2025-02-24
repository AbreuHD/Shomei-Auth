using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Sessions;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infraestructure.Identity.Features.UserSessions.Queries
{
    /// <summary>
    /// Query to retrieve all sessions for a user.
    /// </summary>
    /// <remarks>
    /// This query fetches all active sessions associated with a user, including session details such as the IP address,
    /// user agent, and geo-location information (country and city) based on the IP address.
    /// </remarks>
    public class GetAllUserSessionsQuery : IRequest<GenericApiResponse<List<UserSessionResponse>>>
    {
        /// <summary>
        /// The ID of the user for whom the sessions are to be retrieved.
        /// </summary>
        public required string UserId { get; set; }
    }
    internal class GetAllUserSessionsQueryHandler(IdentityContext identityContext, IHttpClientFactory httpClientFactory) : IRequestHandler<GetAllUserSessionsQuery, GenericApiResponse<List<UserSessionResponse>>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

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
                        Country = "",
                        City = ""
                    })
                    .ToListAsync(cancellationToken);

                foreach (var session in userSessions)
                {
                    var geoInfo = await ExtraMethods.GetGeoLocationInfo(session.IpAddress, _httpClientFactory);
                    if (geoInfo != null)
                    {
                        session.Country = geoInfo.Country;
                        session.City = geoInfo.City;
                    }
                }

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
