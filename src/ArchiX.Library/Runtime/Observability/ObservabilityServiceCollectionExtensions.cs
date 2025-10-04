using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// OpenTelemetry tabanlı gözlemlenebilirlik bileşenlerinin DI kaydı için uzantılar.
/// </summary>
public static class ObservabilityServiceCollectionExtensions
{
    /// <summary>
    /// ArchiX gözlemlenebilirlik (tracing, metrics, logs) yapılandırmasını ekler.
    /// </summary>
    /// <param name="services">DI koleksiyonu.</param>
    /// <param name="configuration">Uygulama yapılandırması.</param>
    /// <param name="environment">Çalışma ortamı.</param>
    /// <returns>Güncellenmiş <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddArchiXObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = new ObservabilityOptions();
        configuration.GetSection("Observability").Bind(options);
        if (!options.Enabled)
        {
            return services;
        }

        var resource = ResourceBuilder.CreateDefault()
            .AddService(serviceName: ArchiXTelemetry.ServiceName, serviceVersion: ArchiXTelemetry.ServiceVersion)
            .AddAttributes([
                new("deployment.environment", (object)environment.EnvironmentName)
            ]);

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                if (!options.Tracing.Enabled) return;

                builder
                    .SetResourceBuilder(resource)
                    .AddSource(ArchiXTelemetry.ServiceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (string.Equals(options.Tracing.Exporter, "otlp", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AddOtlpExporter(o =>
                    {
                        if (!string.IsNullOrWhiteSpace(options.Tracing.OtlpEndpoint))
                        {
                            o.Endpoint = new Uri(options.Tracing.OtlpEndpoint);
                        }
                    });
                }
            })
            .WithMetrics(builder =>
            {
                if (!options.Metrics.Enabled) return;

                builder
                    .SetResourceBuilder(resource)
                    .AddMeter(ArchiXTelemetry.Meter.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (string.Equals(options.Metrics.Exporter, "prometheus", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AddPrometheusExporter();
                }
                else if (string.Equals(options.Metrics.Exporter, "otlp", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AddOtlpExporter(o =>
                    {
                        if (!string.IsNullOrWhiteSpace(options.Metrics.OtlpEndpoint))
                        {
                            o.Endpoint = new Uri(options.Metrics.OtlpEndpoint);
                        }
                    });
                }
            });

        services.AddLogging(loggingBuilder =>
        {
            if (!options.Logs.Enabled) return;

            loggingBuilder.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.ParseStateValues = true;
                o.IncludeFormattedMessage = true;
                o.SetResourceBuilder(resource);

                if (string.Equals(options.Logs.Exporter, "otlp", StringComparison.OrdinalIgnoreCase))
                {
                    o.AddOtlpExporter(otlp =>
                    {
                        if (!string.IsNullOrWhiteSpace(options.Logs.OtlpEndpoint))
                        {
                            otlp.Endpoint = new Uri(options.Logs.OtlpEndpoint);
                        }
                    });
                }
            });
        });

        return services;
    }
}
