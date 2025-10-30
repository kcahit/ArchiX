// File: src/ArchiX.Library/Runtime/Observability/ObservabilityServiceCollectionExtensions.cs
using System.Diagnostics.Metrics;
using System.Reflection;

using ArchiX.Library.Infrastructure.EfCore;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ArchiX.Library.Runtime.Observability
{
    /// <summary>
    /// Observability servis kayıtları.
    /// <para>
    /// Yapılandırma anahtarları:
    /// <list type="bullet">
    /// <item><description><c>Observability:MeterName</c> (string, yoksa <see cref="IHostEnvironment.ApplicationName"/>)</description></item>
    /// <item><description><c>Observability:Metrics:Enabled</c> (bool, varsayılan: true)</description></item>
    /// <item><description><c>Observability:Metrics:Prometheus:Enabled</c> (bool, varsayılan: true)</description></item>
    /// <item><description><c>Observability:Tracing:Enabled</c> (bool, varsayılan: true)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public static class ObservabilityServiceCollectionExtensions
    {
        /// <summary>
        /// Varsayılan sayaç adıyla (<c>ArchiX.Library</c>) Meter ve EF komut metrik kesicisini kaydeder.
        /// Testler bu imzayı çağırır.
        /// </summary>
        public static IServiceCollection AddArchiXObservability(this IServiceCollection services)
            => services.AddArchiXObservability("ArchiX.Library");

        /// <summary>
        /// Verilen sayaç adıyla <see cref="Meter"/> ve <see cref="DbCommandMetricsInterceptor"/> kaydeder.
        /// Servis çözümlemesi yapmaz.
        /// </summary>
        /// <param name="services">Hedef koleksiyon.</param>
        /// <param name="meterName">Meter adı. Boş olamaz.</param>
        public static IServiceCollection AddArchiXObservability(this IServiceCollection services, string meterName)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrEmpty(meterName);

            // Meter tekil olmalı
            services.TryAddSingleton<Meter>(_ => new Meter(meterName));

            // Interceptor hem kendisi hem de taban türü ile çözümlenebilsin
            services.TryAddSingleton<DbCommandMetricsInterceptor>();
            services.TryAddSingleton<DbCommandInterceptor>(sp => sp.GetRequiredService<DbCommandMetricsInterceptor>());

            return services;
        }

        /// <summary>
        /// OpenTelemetry metrik ve izleme için kayıtları yapar. Ayrıca Meter ve EF interceptor'ı ekler.
        /// </summary>
        /// <param name="services">Hedef koleksiyon.</param>
        /// <param name="configuration">Yapılandırma.</param>
        /// <param name="environment">Çalışma ortamı.</param>
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

            // Testlerin beklediği kayıtlar
            services.AddArchiXObservability(meterName);

            var enableMetrics = section.GetValue("Metrics:Enabled", true);
            var enablePrometheus = section.GetValue("Metrics:Prometheus:Enabled", true);
            var enableTracing = section.GetValue("Tracing:Enabled", true);

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
                    if (enablePrometheus)
                        mb.AddPrometheusExporter(); // Endpoint eşleme ayrı sınıfta
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
