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
        /// Exporter yoksa uygulamayı çökertmez; bilgilendirici fallback sağlar.
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
                try
                {
                    // OpenTelemetry Prometheus exporter eklendiyse bu çağrı başarılı olur
                    endpoints.MapPrometheusScrapingEndpoint(customPath);
                }
                catch (InvalidOperationException)
                {
                    // Exporter yoksa uygulamayı çökertmeyelim; bilgilendirici 501 dönen stub
                    endpoints.MapGet(customPath, static async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status501NotImplemented;
                        await context.Response.WriteAsync("Prometheus exporter is not configured.");
                    });
                }

                // Özel bir yol verildiyse, /metrics NOT FOUND dönmeli.
                if (!string.Equals(customPath, "/metrics", StringComparison.OrdinalIgnoreCase))
                {
                    endpoints.MapGet("/metrics", static async context =>
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
