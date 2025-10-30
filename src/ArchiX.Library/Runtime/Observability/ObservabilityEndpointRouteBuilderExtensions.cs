// File: src/ArchiX.Library/Runtime/Observability/ObservabilityEndpointRouteBuilderExtensions.cs
using Microsoft.AspNetCore.Builder;
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
            var path = section.GetValue("Metrics:Prometheus:ScrapeEndpointPath", "/metrics");

            if (enableMetrics && enablePrometheus)
                endpoints.MapPrometheusScrapingEndpoint(path);

            return endpoints;
        }
    }
}
