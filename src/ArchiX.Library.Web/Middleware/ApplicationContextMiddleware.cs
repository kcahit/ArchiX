using ArchiX.Library.Abstractions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Middleware
{
    public sealed class ApplicationContextMiddleware
    {
        private readonly RequestDelegate _next;

        public ApplicationContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApplicationContext appContext)
        {
            if (context.Items.TryGetValue("ApplicationId", out var appIdObj) && appIdObj is int appId)
            {
                appContext.ApplicationId = appId;
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    appContext.UserId = userId;
                }
                appContext.UserName = context.User.Identity?.Name;
            }

            await _next(context);
        }
    }

    public static class ApplicationContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseApplicationContext(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApplicationContextMiddleware>();
        }
    }
}
