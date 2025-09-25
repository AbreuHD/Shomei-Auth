using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infraestructure.Identity.Services
{
    internal class BanService(UserManager<ApplicationUser> userManager)
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<GenericApiResponse<bool>> BanUserAsync(string userId, TimeSpan? duration = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new GenericApiResponse<bool>
                {
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "User not found",
                    Success = false,
                    Payload = false
                };
            }

            user.LockoutEnd = duration.HasValue ? DateTimeOffset.UtcNow.Add(duration.Value) : DateTimeOffset.MaxValue;
            user.IsBanned = true;

            await _userManager.UpdateAsync(user);

            return new GenericApiResponse<bool>
            {
                Statuscode = StatusCodes.Status200OK,
                Message = duration.HasValue
                    ? $"User banned for {duration.Value.TotalMinutes} minutes"
                    : "User permanently banned",
                Success = true,
                Payload = true
            };
        }

        public async Task<GenericApiResponse<bool>> UnbanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new GenericApiResponse<bool>
                {
                    Statuscode = StatusCodes.Status404NotFound,
                    Message = "User not found",
                    Success = false,
                    Payload = false
                };
            }

            user.LockoutEnd = null; // Elimina el bloqueo
            user.IsBanned = false;

            await _userManager.UpdateAsync(user);

            return new GenericApiResponse<bool>
            {
                Statuscode = StatusCodes.Status200OK,
                Message = "User unbanned successfully",
                Success = true,
                Payload = true
            };
        }
    }
}
