// File: src/ArchiX.Library/Infrastructure/CachingServiceCollectionExtensions.cs
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Infrastructure
{
    /// <summary>
    /// Önbellek (caching) servisleri ve ilgili yapılandırmalar için DI kayıt uzantıları.
    /// </summary>
    public static class CachingServiceCollectionExtensions
    {
        /// <summary>
        /// In-memory caching’i (<see cref="ICacheService"/>) ve
        /// <see cref="ICacheService"/> implementasyonunu DI konteynerine ekler.
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
        /// <see cref="IRedisCacheService"/> implementasyonunu DI konteynerine ekler.
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

            services.AddSingleton<IRedisCacheService, RedisCacheService>();
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
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            return services;
        }

        // ===================== 4,0210 — Redis Serialization Options =====================

        /// <summary>
        /// Redis serileştirme davranışını (System.Text.Json) <see cref="RedisSerializationOptions"/> üzerinden yapılandırır.
        /// </summary>
        /// <remarks>
        /// <para>Örnek:</para>
        /// <code>
        /// services.AddArchiXRedisSerialization(options =>
        /// {
        ///     options.Json.PropertyNamingPolicy = null; // PascalCase
        ///     options.Json.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        /// });
        /// </code>
        /// </remarks>
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
        /// Yalnızca <see cref="System.Text.Json.JsonSerializerOptions"/> özelleştirmek için kısayol.
        /// </summary>
        /// <remarks>
        /// <para>Örnek:</para>
        /// <code>
        /// services.AddArchiXRedisSerialization(json =>
        /// {
        ///     json.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        ///     json.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        /// });
        /// </code>
        /// </remarks>
        public static IServiceCollection AddArchiXRedisSerialization(
            this IServiceCollection services,
            Action<System.Text.Json.JsonSerializerOptions> configureJson)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configureJson);

            services.Configure<RedisSerializationOptions>(opts => configureJson(opts.Json));
            return services;
        }
    }
}
