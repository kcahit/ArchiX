// File: src/ArchiX.Library/Runtime/Observability/ObservabilityEndpointRouteBuilderExtensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace ArchiX.Library.Runtime.Observability
{
    /// <summary>
    /// Observability uç nokta eşlemeleri.
    /// </summary>
    public static class ObservabilityEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Prometheus scraping endpoint’ini bayraklara göre eşler.
        /// </summary>
        public static IEndpointRouteBuilder MapArchiXObservability(
            this IEndpointRouteBuilder endpoints,
            IConfiguration configuration)
        {
            var section = configuration.GetSection("Observability");

            var enableMetrics = section.GetValue("Metrics:Enabled", true);
            var enablePrometheus = section.GetValue("Metrics:Prometheus:Enabled", true);

            // Anahtar uyumluluğu ve varsayılan.
            var customPath =
                section.GetValue<string?>("Metrics:Prometheus:ScrapeEndpointPath")
                ?? section.GetValue<string?>("Metrics:Prometheus:ScrapeEndpoint")
                ?? section.GetValue<string?>("Metrics:Prometheus:Path")
                ?? "/metrics";

            // Başında "/" yoksa ekle
            if (!string.IsNullOrWhiteSpace(customPath) && !customPath!.StartsWith('/'))
                customPath = "/" + customPath;

            if (enableMetrics && enablePrometheus)
            {
                // Yalnızca belirtilen path'i Prometheus için eşle.
                endpoints.MapPrometheusScrapingEndpoint(customPath);

                // Test beklentisi: Özel bir yol verildiyse, /metrics NOT FOUND dönmeli.
                // Bu nedenle, customPath "/metrics" değilse, /metrics'i 404'e sabitle.
                if (!string.Equals(customPath, "/metrics", StringComparison.OrdinalIgnoreCase))
                {
                    endpoints.MapGet("/metrics", async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.CompleteAsync();
                    });
                }
            }

            return endpoints;
        }
    }
}
