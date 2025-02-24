using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.DTOs.Generic;
using Auth.Infraestructure.Identity.Entities;
using Auth.Infraestructure.Identity.Extra;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Infraestructure.Identity.Middleware
{
    /// <summary>
    /// A custom authorization filter that checks if the user has a valid session by verifying the JWT token.
    /// This attribute can be applied to both classes and methods. It ensures that the user's session is valid,
    /// and if not, returns an unauthorized response (HTTP 401).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MultipleSessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {

        /// <summary>
        /// Asynchronously checks if the user is authorized to access the resource.
        /// It validates the authorization header, token, and session status.
        /// If any validation fails, an unauthorized response is returned.
        /// </summary>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var dbContext = context.HttpContext.RequestServices.GetService(typeof(IdentityContext)) as IdentityContext;
            var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();

            if (!IsValidAuthHeader(authHeader, out var token) ||
                !TryGetUserIdFromToken(new JwtSecurityTokenHandler(), token, out var userId) ||
                !await IsValidSession(dbContext, token, userId))
            {
                context.Result = UnauthorizedResponse();
            }
        }

        private static ObjectResult UnauthorizedResponse()
        {
            return new ObjectResult(new GenericApiResponse<bool>
            {
                Success = false,
                Statuscode = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized",
                Payload = false
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        private static bool IsValidAuthHeader(string authHeader, out string token)
        {
            token = string.Empty;
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return false;

            token = authHeader.Split(" ")[^1];
            return true;
        }

        private static bool TryGetUserIdFromToken(JwtSecurityTokenHandler tokenHandler, string token, out string userId)
        {
            userId = string.Empty;
            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
                return !string.IsNullOrEmpty(userId);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> IsValidSession(DbContext dbContext, string token, string userId)
        {
            var tokenHased = ExtraMethods.HashToken(token);
            var session = await dbContext.Set<UserSession>()
                .FirstOrDefaultAsync(s => s.Token == tokenHased && s.UserId == userId);

            return session != null && session.Expiration >= DateTime.UtcNow;
        }
    }
}