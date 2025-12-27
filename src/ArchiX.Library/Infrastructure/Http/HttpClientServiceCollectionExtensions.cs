// File: src/ArchiX.Library/Infrastructure/Http/HttpClientServiceCollectionExtensions.cs
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ArchiX.Library.Abstractions.Http;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// <see cref="IServiceCollection"/> için HttpClient wrapper DI kayıt uzantıları.
    /// </summary>
    public static class HttpClientServiceCollectionExtensions
    {
        /// <summary>
        /// Tipik bir wrapper implementasyonunu typed HttpClient olarak kaydeder.
        /// </summary>
        /// <typeparam name="TWrapper">IHttpClientWrapper implementasyonu (örn. DefaultHttpClientWrapper veya özel client sınıfın).</typeparam>
        /// <param name="services">DI koleksiyonu.</param>
        /// <param name="configureClient">HttpClient yapılandırması (BaseAddress, Timeout, DefaultHeaders vb.).</param>
        /// <param name="configureJson">JSON serileştirme seçenekleri (opsiyonel).</param>
        /// <param name="_">İmza uyumluluğu için ayrılmış parametre (kullanılmıyor).</param>
        /// <returns>DI koleksiyonu.</returns>
        public static IServiceCollection AddHttpClientWrapper<TWrapper>(
            this IServiceCollection services,
            Action<HttpClient> configureClient,
            Action<JsonSerializerOptions>? configureJson = null,
            int _ = 0)
            where TWrapper : class, IHttpClientWrapper
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configureClient);

            // JSON seçenekleri
            services.AddSingleton(_ =>
            {
                var opts = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };
                configureJson?.Invoke(opts);
                return opts;
            });

            services.AddHttpClient<TWrapper>(configureClient);
            services.AddScoped<IHttpClientWrapper, TWrapper>();
            return services;
        }

        /// <summary>
        /// Handler zinciri (**Logging → ProblemDetails → Correlation → Retry → Timeout**) ile typed HttpClient kaydı.
        /// </summary>
        /// <typeparam name="TWrapper">IHttpClientWrapper implementasyonu.</typeparam>
        /// <param name="services">DI koleksiyonu.</param>
        /// <param name="configureClient">HttpClient yapılandırması.</param>
        /// <param name="maxRetries">Retry deneme sayısı (varsayılan: 3).</param>
        /// <param name="baseDelay">Retry taban gecikmesi (varsayılan: 200ms).</param>
        /// <param name="timeout">İstek başına süre sınırı (varsayılan: 30s).</param>
        /// <param name="configureJson">JSON serileştirme seçenekleri (opsiyonel).</param>
        /// <returns>DI koleksiyonu.</returns>
        public static IServiceCollection AddHttpClientWrapperWithPolicies<TWrapper>(
            this IServiceCollection services,
            Action<HttpClient> configureClient,
            int maxRetries = 3,
            TimeSpan? baseDelay = null,
            TimeSpan? timeout = null,
            Action<JsonSerializerOptions>? configureJson = null)
            where TWrapper : class, IHttpClientWrapper
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configureClient);

            // JSON seçenekleri
            services.AddSingleton(_ =>
            {
                var opts = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };
                configureJson?.Invoke(opts);
                return opts;
            });

            // Handler DI kayıtları
            services.AddTransient<OutboundLoggingHandler>(); // OUTERMOST
            services.AddTransient<ProblemDetailsHandler>();  // Hata eşleme
            services.AddTransient<CorrelationHandler>();     // Korelasyon

            // Typed HttpClient + handler zinciri
            services
                .AddHttpClient<TWrapper>(configureClient)
                .AddHttpMessageHandler<OutboundLoggingHandler>()                                      // 1) Logging
                .AddHttpMessageHandler<ProblemDetailsHandler>()                                       // 2) ProblemDetails
                .AddHttpMessageHandler<CorrelationHandler>()                                          // 3) Correlation
                .AddHttpMessageHandler(sp => new RetryHandler(maxRetries, baseDelay))                 // 4) Retry
                .AddHttpMessageHandler(sp => new TimeoutHandler(timeout ?? TimeSpan.FromSeconds(30))); // 5) Timeout

            services.AddScoped<IHttpClientWrapper, TWrapper>();
            return services;
        }
    }
}
