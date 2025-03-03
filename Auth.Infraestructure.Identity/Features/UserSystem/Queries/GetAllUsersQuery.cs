using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.DTOs.Users;
using Auth.Infraestructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Auth.Infraestructure.Identity.Features.UserSystem.Queries
{
    public class GetAllUsersQuery : IRequest<GenericApiResponse<List<GetAllUsersResponseDto>>>
    {
    }
    internal class GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetAllUsersQuery, GenericApiResponse<List<GetAllUsersResponseDto>>>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        public async Task<GenericApiResponse<List<GetAllUsersResponseDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var response = new GenericApiResponse<List<GetAllUsersResponseDto>>
            {
                Message = "Success",
                Payload = [],
                Statuscode = StatusCodes.Status200OK,
                Success = true
            };
            try
            {
                var users = await _userManager.Users.ToListAsync(cancellationToken: cancellationToken);
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    response.Payload.Add(new GetAllUsersResponseDto
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email!,
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumber = user.PhoneNumber ?? "",
                        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                        IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                        LockoutEnd = user.LockoutEnd,
                        AccessFailedCount = user.AccessFailedCount,
                        Roles = [.. roles],
                        isBanned = user.IsBanned ?? false
                    });
                }
            }catch (Exception e)
            {
                response.Success = false;
                response.Statuscode = 500;
                response.Message = e.Message;
                response.Statuscode = StatusCodes.Status500InternalServerError;
            }

            return response;
        }
    }
}
