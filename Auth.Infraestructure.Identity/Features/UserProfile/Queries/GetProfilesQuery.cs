﻿using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Profiles;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infraestructure.Identity.Features.UserProfile.Queries
{
    /// <summary>
    /// Query for retrieving user profiles based on the user ID.
    /// </summary>
    /// <remarks>
    /// This query is used to fetch all profiles associated with a specific user.
    /// </remarks>
    public class GetProfilesQuery : IRequest<GenericApiResponse<List<GetProfilesResponse>>>
    {
    }

    internal class GetProfilesQueryHandler(IdentityContext identityContext, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetProfilesQuery, GenericApiResponse<List<GetProfilesResponse>>>
    {
        private readonly IdentityContext _identityContext = identityContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        public async Task<GenericApiResponse<List<GetProfilesResponse>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
        {
            var UserId = _httpContextAccessor.HttpContext.User.FindFirst("uid")?.Value ?? "Unknown";
            var response = new GenericApiResponse<List<GetProfilesResponse>>()
            {
                Success = false,
                Statuscode = StatusCodes.Status500InternalServerError,
                Message = "N/A"
            };
            var profiles = await _identityContext.Set<Entities.UserProfile>().Where(x => x.UserId == UserId).ToListAsync();
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
                AvatarUrl = x.AvatarUrl,
                UsePassword = x.Password != null
            })];
            return response;
        }
    }
}
