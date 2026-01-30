using ArchiX.Library.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Web.Middleware
{
    public sealed class ArchiXMiddleware
    {
        private readonly RequestDelegate _next;

        public ArchiXMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<ArchiXOptions> optionsAccessor)
        {
            var opts = optionsAccessor.Value;
            var host = context.Request.Host.Value;
            int applicationId;
            if (!string.IsNullOrWhiteSpace(host) && opts.HostApplicationMapping.TryGetValue(host, out var mapped))
            {
                applicationId = mapped;
            }
            else
            {
                applicationId = opts.DefaultApplicationId;
            }

            context.Items["ApplicationId"] = applicationId;
            await _next(context);
        }
    }

    public static class ArchiXMiddlewareExtensions
    {
        public static IApplicationBuilder UseArchiX(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ArchiXMiddleware>();
        }
    }
}
