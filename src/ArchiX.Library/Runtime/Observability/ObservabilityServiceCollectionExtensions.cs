// File: src/ArchiX.Library/Runtime/Observability/ObservabilityServiceCollectionExtensions.cs
using System.Diagnostics.Metrics;
using System.Reflection;

using ArchiX.Library.Infrastructure.EfCore;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ArchiX.Library.Runtime.Observability
{
    public static class ObservabilityServiceCollectionExtensions
    {
        public static IServiceCollection AddArchiXObservability(this IServiceCollection services)
            => services.AddArchiXObservability("ArchiX.Library");

        public static IServiceCollection AddArchiXObservability(this IServiceCollection services, string meterName)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrEmpty(meterName);

            // Tek bir Meter instance
            services.TryAddSingleton<Meter>(_ => new Meter(meterName));

            // EF Core interceptor
            services.TryAddSingleton<DbCommandMetricsInterceptor>();
            services.TryAddSingleton<DbCommandInterceptor>(sp => sp.GetRequiredService<DbCommandMetricsInterceptor>());

            // Metrik servisleri
            services.TryAddSingleton<ErrorMetric>();
            services.TryAddSingleton<DbMetric>();

            return services;
        }

        public static IServiceCollection AddArchiXObservability(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);

            var section = configuration.GetSection("Observability");

            var meterName = section.GetValue<string?>("MeterName")
                ?? environment.ApplicationName
                ?? "ArchiX";

            services.AddArchiXObservability(meterName);

            var enableMetrics = section.GetValue("Metrics:Enabled", true);
            var enablePrometheus = section.GetValue("Metrics:Prometheus:Enabled", true);
            var enableTracing = section.GetValue("Tracing:Enabled", true);

            // Scrape path konfigürasyonu (testin kullandığı anahtarlar dahil)
            // Öncelik: ScrapeEndpointPath → ScrapeEndpoint → Path → "/metrics"
            var scrapePath =
                section.GetValue<string?>("Metrics:Prometheus:ScrapeEndpointPath")
                ?? section.GetValue<string?>("Metrics:Prometheus:ScrapeEndpoint")
                ?? section.GetValue<string?>("Metrics:Prometheus:Path")
                ?? section.GetValue<string?>("Metrics:ScrapeEndpoint") // test burayı veriyor
                ?? "/metrics";

            if (!string.IsNullOrWhiteSpace(scrapePath) && !scrapePath.StartsWith('/'))
                scrapePath = "/" + scrapePath;

            var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";

            var otel = services.AddOpenTelemetry();
            otel.ConfigureResource(rb => rb.AddService(meterName, serviceVersion: version));

            if (enableMetrics)
            {
                otel.WithMetrics(mb =>
                {
                    mb.AddAspNetCoreInstrumentation();
                    mb.AddHttpClientInstrumentation();
                    mb.AddRuntimeInstrumentation();


                    // Kritik ekleme: yalnızca bu meter export edilsin
                    mb.AddMeter(meterName, ArchiXTelemetry.ServiceName);

                    if (enablePrometheus)
                    {
                        // Varsayılan /metrics yerine konfigürde edilen özel yol
                        mb.AddPrometheusExporter(options =>
                        {
                            options.ScrapeEndpointPath = scrapePath;
                        });
                    }
                });
            }

            if (enableTracing)
            {
                otel.WithTracing(tb =>
                {
                    tb.AddAspNetCoreInstrumentation();
                    tb.AddHttpClientInstrumentation();
                });
            }

            return services;
        }
    }
}
