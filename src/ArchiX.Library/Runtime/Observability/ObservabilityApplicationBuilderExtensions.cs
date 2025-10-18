using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// Pipeline seviyesinde gözlemlenebilirlik entegrasyonları için uzantılar.
/// </summary>
public static class ObservabilityApplicationBuilderExtensions
{
    /// <summary>
    /// Konfigürasyona göre Prometheus scraping endpoint ve benzeri entegrasyonları ekler.
    /// </summary>
    /// <param name="app">Uygulama oluşturucu.</param>
    /// <param name="configuration">Yapılandırma.</param>
    /// <returns><see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseArchiXObservability(this IApplicationBuilder app, IConfiguration configuration)
    {
        var options = new ObservabilityOptions();
        configuration.GetSection("Observability").Bind(options);
        if (!options.Enabled) return app;

        if (options.Metrics.Enabled &&
            string.Equals(options.Metrics.Exporter, "prometheus", System.StringComparison.OrdinalIgnoreCase))
        {
            app.UseOpenTelemetryPrometheusScrapingEndpoint(options.Metrics.ScrapeEndpoint ?? "/metrics");
        }

        return app;
    }
}
