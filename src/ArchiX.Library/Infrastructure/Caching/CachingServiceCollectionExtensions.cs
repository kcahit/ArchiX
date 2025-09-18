// File: src/ArchiX.Library/Infrastructure/CachingServiceCollectionExtensions.cs
using System.Text.Json;

using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// Önbellek (caching) servisleri, serileştirme seçenekleri ve cache key politikası için DI kayıt uzantıları.
    /// </summary>
    public static class CachingServiceCollectionExtensions
    {
        /// <summary>
        /// In-memory caching’i (<see cref="ICacheService"/>) ve implementasyonunu DI’a ekler.
        /// </summary>
        public static IServiceCollection AddArchiXMemoryCaching(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            return services;
        }

        /// <summary>
        /// Redis (StackExchange.Redis tabanlı <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>) için
        /// <see cref="ICacheService"/> implementasyonunu DI’a ekler.
        /// </summary>
        /// <param name="services">DI konteyneri.</param>
        /// <param name="configuration">Örn: "localhost:6379,abortConnect=false".</param>
        /// <param name="instanceName">Opsiyonel instance adı; key prefix olarak kullanılır.</param>
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

            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }

        /// <summary>
        /// Redis kaydı için gelişmiş seçenekler ile yapılandırma imkânı sunar.
        /// </summary>
        public static IServiceCollection AddArchiXRedisCaching(
            this IServiceCollection services,
            Action<RedisCacheOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddStackExchangeRedisCache(configure);
            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }

        // ===================== 4,0210 — Redis Serialization Options =====================

        /// <summary>
        /// Redis serileştirme davranışını (System.Text.Json) <see cref="RedisSerializationOptions"/> üzerinden yapılandırır.
        /// </summary>
        public static IServiceCollection AddArchiXRedisSerialization(
            this IServiceCollection services,
            Action<RedisSerializationOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.Configure(configure);
            return services;
        }

        /// <summary>
        /// Yalnızca <see cref="JsonSerializerOptions"/> özelleştirmek için kısayol.
        /// </summary>
        public static IServiceCollection AddArchiXRedisSerialization(
            this IServiceCollection services,
            Action<JsonSerializerOptions> configureJson)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configureJson);

            services.Configure<RedisSerializationOptions>(opts => configureJson(opts.Json));
            return services;
        }

        // ===================== 4,022 — Cache Key Policy =====================

        /// <summary>
        /// Cache key politikası için varsayılan ayarlarla <see cref="ICacheKeyPolicy"/> kaydeder.
        /// </summary>
        public static IServiceCollection AddArchiXCacheKeyPolicy(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddOptions<CacheKeyPolicyOptions>();

            services.AddSingleton<ICacheKeyPolicy>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<CacheKeyPolicyOptions>>().Value;
                return new DefaultCacheKeyPolicy(opt);
            });

            return services;
        }

        /// <summary>
        /// Cache key politikasını verilen yapılandırmayla kaydeder (prefix/tenant/culture/version).
        /// </summary>
        public static IServiceCollection AddArchiXCacheKeyPolicy(
            this IServiceCollection services,
            Action<CacheKeyPolicyOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddOptions<CacheKeyPolicyOptions>()
                    .Configure(configure);

            services.AddSingleton<ICacheKeyPolicy>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<CacheKeyPolicyOptions>>().Value;
                return new DefaultCacheKeyPolicy(opt);
            });

            return services;
        }

        /// <summary>
        /// Cache key politikasını <see cref="IConfiguration"/> üzerinden bağlar ve kaydeder.
        /// Varsayılan bölüm adı: <c>"ArchiX:CacheKeyPolicy"</c>.
        /// </summary>
        public static IServiceCollection AddArchiXCacheKeyPolicy(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "ArchiX:CacheKeyPolicy")
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var section = configuration.GetSection(sectionName);

            services.AddOptions<CacheKeyPolicyOptions>()
                    .Bind(section);

            services.AddSingleton<ICacheKeyPolicy>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<CacheKeyPolicyOptions>>().Value;
                return new DefaultCacheKeyPolicy(opt);
            });

            return services;
        }
    }
}
