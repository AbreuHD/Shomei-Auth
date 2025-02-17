using Auth.Infraestructure.Identity.DTOs.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auth.Infraestructure.Identity.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ClaimRequiredAttribute(string claimType, string message) : ActionFilterAttribute
    {
        private readonly string _claimType = claimType;
        private readonly string _message = message;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true || !user.Claims.Any(c => c.Type == _claimType))
            {
                context.Result = new NotFoundObjectResult(new GenericApiResponse<bool> { Message = _message, Success = false, Payload = false, Statuscode = 401 });
            }

            base.OnActionExecuting(context);
        }
    }
}
