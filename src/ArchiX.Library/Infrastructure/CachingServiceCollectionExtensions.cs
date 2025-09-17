// File: src/ArchiX.Library/Infrastructure/CachingServiceCollectionExtensions.cs
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Infrastructure
{
    /// <summary>
    /// Önbellek (caching) servisleri için DI kayıt uzantılarını içerir.
    /// <para>
    /// Bu uzantılar aracılığıyla uygulama; in-memory (<see cref="IMemoryCache"/>) ve
    /// dağıtık (Redis) önbellek implementasyonlarını kolayca kaydedebilir.
    /// </para>
    /// </summary>
    public static class CachingServiceCollectionExtensions
    {
        /// <summary>
        /// In-memory caching’i (<see cref="IMemoryCache"/>) ve
        /// <see cref="IMemoryCacheService"/> implementasyonunu DI konteynerine ekler.
        /// </summary>
        /// <param name="services">Bağımlılık enjeksiyonu (DI) konteyneri.</param>
        /// <returns>Değiştirilen <paramref name="services"/> örneği.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> null ise fırlatılır.
        /// </exception>
        public static IServiceCollection AddArchiXMemoryCaching(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddMemoryCache();
            services.AddSingleton<IMemoryCacheService, MemoryCacheService>();
            return services;
        }

        /// <summary>
        /// Redis (StackExchange.Redis tabanlı <see cref="IDistributedCache"/>) için
        /// <see cref="IRedisCacheService"/> implementasyonunu DI konteynerine ekler.
        /// </summary>
        /// <param name="services">Bağımlılık enjeksiyonu (DI) konteyneri.</param>
        /// <param name="configuration">
        /// StackExchange.Redis bağlantı dizesi (örn. <c>"localhost:6379,abortConnect=false"</c>).
        /// </param>
        /// <param name="instanceName">
        /// Opsiyonel instance adı; Redis anahtarları için prefix olarak kullanılır.
        /// </param>
        /// <returns>Değiştirilen <paramref name="services"/> örneği.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> veya <paramref name="configuration"/> null ise fırlatılır.
        /// </exception>
        public static IServiceCollection AddArchiXRedisCaching(
            this IServiceCollection services,
            string configuration,
            string? instanceName = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = configuration;
                if (!string.IsNullOrWhiteSpace(instanceName))
                    opts.InstanceName = instanceName;
            });

            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            return services;
        }

        /// <summary>
        /// Redis kaydı için gelişmiş seçenekler ile yapılandırma imkânı sunar.
        /// </summary>
        /// <param name="services">Bağımlılık enjeksiyonu (DI) konteyneri.</param>
        /// <param name="configure">
        /// Redis seçeneklerini yapılandıran temsilci
        /// (<see cref="Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions"/>).
        /// </param>
        /// <returns>Değiştirilen <paramref name="services"/> örneği.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> veya <paramref name="configure"/> null ise fırlatılır.
        /// </exception>
        public static IServiceCollection AddArchiXRedisCaching(
            this IServiceCollection services,
            Action<Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddStackExchangeRedisCache(configure);
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            return services;
        }
    }
}
