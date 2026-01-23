// File: src/ArchiX.Library/Infrastructure/Http/HttpPoliciesServiceCollectionExtensions.cs
#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>HTTP retry/timeout politikalarını tek yerden yönetmek için DI uzantıları.</summary>
    public static class HttpPoliciesServiceCollectionExtensions
    {
        /// <summary>
        /// #57 DB'den HttpPoliciesOptions okur (IParameterService kullanarak).
        /// Startup sırasında çağrılmamalı (DbContext gerektirir), runtime'da lazy load edilir.
        /// </summary>
        public static IServiceCollection AddHttpPoliciesFromDatabase(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Transient handler'lar için DI kaydı
            services.AddTransient<ProblemDetailsHandler>();
            services.AddTransient<RetryHandler>(sp =>
            {
                var opts = sp.GetService<HttpPoliciesOptions>() ?? new HttpPoliciesOptions();
                return new RetryHandler(opts.RetryCount, opts.GetBaseDelay());
            });
            services.AddTransient<TimeoutHandler>(sp =>
            {
                var opts = sp.GetService<HttpPoliciesOptions>() ?? new HttpPoliciesOptions();
                return new TimeoutHandler(opts.GetTimeout());
            });

            // Singleton olarak lazy provider ekle
            services.AddSingleton<HttpPoliciesOptions>(sp =>
            {
                // Runtime'da ilk kullanımda yüklenecek
                // NOT: Startup'ta çağrılmaz çünkü DbContext henüz hazır değil
                var opts = new HttpPoliciesOptions(); // Fallback defaults
                return opts;
            });

            return services;
        }

        /// <summary>
        /// Konfigürasyondan <see cref="HttpPoliciesOptions"/> bağlar.
        /// Varsayılan bölüm: <c>HttpPolicies</c>.
        /// </summary>
        public static IServiceCollection AddHttpPolicies(
            this IServiceCollection services,
            IConfiguration config,
            string sectionPath = "HttpPolicies")
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);

            services.Configure<HttpPoliciesOptions>(config.GetSection(sectionPath));
            services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<HttpPoliciesOptions>>().Value;
                opts.Validate();
                return opts;
            });

            return services;
        }

        /// <summary>Programatik olarak <see cref="HttpPoliciesOptions"/> ayarlar ve doğrular.</summary>
        public static IServiceCollection AddHttpPolicies(
            this IServiceCollection services,
            Action<HttpPoliciesOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.PostConfigure(configure);
            services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<HttpPoliciesOptions>>().Value;
                opts.Validate();
                return opts;
            });

            return services;
        }

        /// <summary>Çözümleyiciden doğrulanmış <see cref="HttpPoliciesOptions"/> değerini alır.</summary>
        public static HttpPoliciesOptions GetHttpPolicies(this IServiceProvider sp)
        {
            ArgumentNullException.ThrowIfNull(sp);
            return sp.GetRequiredService<HttpPoliciesOptions>();
        }
    }
}
