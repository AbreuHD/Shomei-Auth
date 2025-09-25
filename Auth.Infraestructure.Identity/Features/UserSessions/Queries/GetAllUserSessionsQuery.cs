using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shomei.Infraestructure.Identity.Context;
using Shomei.Infraestructure.Identity.DTOs.Generic;
using Shomei.Infraestructure.Identity.DTOs.Sessions;
using Shomei.Infraestructure.Identity.Entities;
using Shomei.Infraestructure.Identity.Extra;

namespace Shomei.Infraestructure.Identity.Features.UserSessions.Queries
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
    }
    internal class GetAllUserSessionsQueryHandler(IdentityContext identityContext, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetAllUserSessionsQuery, GenericApiResponse<List<UserSessionResponse>>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<GenericApiResponse<List<UserSessionResponse>>> Handle(GetAllUserSessionsQuery request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext.User.FindFirst("uid").Value;
            try
            {
                var userSessions = await _identityContext.Set<UserSession>()
                    .Where(x => x.UserId == UserId)
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
