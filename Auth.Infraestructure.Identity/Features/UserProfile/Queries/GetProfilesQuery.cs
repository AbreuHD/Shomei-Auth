using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Profiles;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Queries
{
    public class GetProfilesQuery : IRequest<GenericApiResponse<List<GetProfilesResponse>>>
    {
        public required string UserId { get; set; }
    }

    internal class GetProfilesQueryHandler(IdentityContext identityContext) : IRequestHandler<GetProfilesQuery, GenericApiResponse<List<GetProfilesResponse>>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        public async Task<GenericApiResponse<List<GetProfilesResponse>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<List<GetProfilesResponse>>();
            var profiles = await _identityContext.Set<Entities.UserProfile>().Where(x => x.UserId == request.UserId).ToListAsync();
            if (profiles.Count == 0)
            {
                response.Success = false;
                response.Message = "No profiles found";
                response.Statuscode = StatusCodes.Status404NotFound;
                return response;
            }
            response.Success = true;
            response.Message = "Profiles found";
            response.Statuscode = StatusCodes.Status200OK;
            response.Payload = [.. profiles.Select(x => new GetProfilesResponse
            {
                Id = x.Id,
                Name = x.Name,
                AvatarUrl = x.AvatarUrl
            })];
            return response;
        }
    }
}
