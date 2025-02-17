using Auth.Infraestructure.Identity.Context;
using Auth.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Infraestructure.Identity.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MultipleSessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var dbContext = context.HttpContext.RequestServices.GetService(typeof(IdentityContext)) as IdentityContext;
            var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                var token = authHeader.Split(" ")[^1];
                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (userId == null)
                    {
                        context.Result = new UnauthorizedResult();
                    }
                    else
                    {
                        var session = await dbContext.Set<UserSession>()
                            .FirstOrDefaultAsync(s => s.Token == token && s.UserId == userId);

                        if (session == null || session.Expiration < DateTime.UtcNow)
                        {
                            context.Result = new UnauthorizedResult();
                        }
                    }
                }
                catch
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }
}
