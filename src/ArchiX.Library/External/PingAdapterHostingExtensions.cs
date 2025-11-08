// File: src/ArchiX.Library/External/PingAdapterHostingExtensions.cs
#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.External
{
    /// <summary>Ping adapteri için barındırma uzantıları.</summary>
    public static class PingAdapterHostingExtensions
    {
        /// <summary>
        /// Tek çağrıyla <see cref="ArchiX.Library.Abstractions.External.IPingAdapter"/> kaydı ve <c>PingHealthCheck</c> ekler.
        /// Varsayılan konfig yolu: <c>ExternalServices:Ping</c>, health check adı: <c>external_ping</c>.
        /// </summary>
        /// <param name="services">DI koleksiyonu.</param>
        /// <param name="config">Uygulama konfigürasyonu.</param>
        /// <param name="sectionPath">Konfigürasyon bölüm yolu.</param>
        /// <param name="healthCheckName">Health check görünen adı.</param>
        public static IServiceCollection AddPingAdapterWithHealthCheck(
            this IServiceCollection services,
            IConfiguration config,
            string sectionPath = "ExternalServices:Ping",
            string healthCheckName = "external_ping")
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);
            services.AddPingAdapter(config, sectionPath);
            services.AddHealthChecks().AddCheck<PingHealthCheck>(healthCheckName);
            return services;
        }
    }
}
