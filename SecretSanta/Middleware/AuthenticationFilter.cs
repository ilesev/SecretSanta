using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SecretSanta.Repository;

namespace SecretSanta.Filter
{
    public class AuthenticationFilter : ActionFilterAttribute
    {
        private IUsersRepository Repository { get; set; }

        public AuthenticationFilter(IUsersRepository Repository)
        {
            this.Repository = Repository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            StringValues authToken;
            if(!context.HttpContext.Request.Headers.TryGetValue("AuthenticationToken",out authToken) ||
               !await Repository.isGuidPresentAsync(authToken[0])) 
            {
                context.HttpContext.Response.ContentType = "text/plain";
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.HttpContext.Response.WriteAsync("Unauthorized access is not allowed.");
                return;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
    