// File: src/ArchiX.Library/External/PingAdapterServiceCollectionExtensions.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

using ArchiX.Library.Infrastructure.Http; // CorrelationHandler, OutboundLoggingHandler, RetryHandler, TimeoutHandler, ProblemDetailsHandler

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.External
{
    /// <summary>Ping adapter DI kayıt uzantıları.</summary>
    public static class PingAdapterServiceCollectionExtensions
    {
        /// <summary>
        /// IPingAdapter’ı tipli HttpClient ile kaydeder.
        /// Correlation → OutboundLogging → Retry → Timeout → ProblemDetails zinciri bağlanır.
        /// </summary>
        /// <param name="services">DI koleksiyonu.</param>
        /// <param name="baseAddress">Dış servisin kök adresi.</param>
        /// <param name="timeout">İsteğe bağlı zaman aşımı.</param>
        public static IServiceCollection AddPingAdapter(
            this IServiceCollection services,
            Uri baseAddress,
            TimeSpan? timeout = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(baseAddress);

            // Handler bağımlılıklarını kaydet (idempotent)
            services.AddTransient<CorrelationHandler>();
            services.AddTransient<OutboundLoggingHandler>();
            services.AddTransient<RetryHandler>();
            services.AddTransient<TimeoutHandler>();
            services.AddTransient<ProblemDetailsHandler>();

            services.AddHttpClient<IPingAdapter, PingAdapter>(client =>
            {
                client.BaseAddress = baseAddress;
                if (timeout is { } t) client.Timeout = t;
                client.DefaultRequestHeaders.Accept.ParseAdd("text/plain");
            })
                .AddHttpMessageHandler<CorrelationHandler>()
                .AddHttpMessageHandler<OutboundLoggingHandler>()
                .AddHttpMessageHandler<RetryHandler>()
                .AddHttpMessageHandler<TimeoutHandler>()
                .AddHttpMessageHandler<ProblemDetailsHandler>();

            return services;
        }

        /// <summary>
        /// Konfigürasyondan IPingAdapter kaydı yapar.
        /// Varsayılan bölüm: <c>ExternalServices:DemoApi</c>.
        /// <list type="bullet">
        /// <item><description><c>BaseAddress</c> (zorunlu, URL)</description></item>
        /// <item><description><c>TimeoutSeconds</c> (opsiyonel, 1–300)</description></item>
        /// </list>
        /// </summary>
        /// <param name="services">DI koleksiyonu.</param>
        /// <param name="config">Uygulama konfigürasyonu.</param>
        /// <param name="sectionPath">Konfigürasyon bölüm yolu.</param>
        public static IServiceCollection AddPingAdapter(
            this IServiceCollection services,
            IConfiguration config,
            string sectionPath = "ExternalServices:DemoApi")
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(config);

            var section = config.GetSection(sectionPath);
            var opts = section.Get<PingAdapterOptions>()
                       ?? throw new InvalidOperationException($"{sectionPath} bölümü okunamadı.");

            ValidateOptions(opts, sectionPath);

            var baseUri = opts.GetBaseUri();
            var timeout = opts.GetTimeout();

            return services.AddPingAdapter(baseUri, timeout);
        }

        /// <summary>DataAnnotations ile seçenekleri doğrular.</summary>
        private static void ValidateOptions(PingAdapterOptions opts, string sectionPath)
        {
            var ctx = new ValidationContext(opts);
            try
            {
                Validator.ValidateObject(opts, ctx, validateAllProperties: true);
            }
            catch (ValidationException vex)
            {
                throw new InvalidOperationException($"Geçersiz {sectionPath} yapılandırması: {vex.Message}", vex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Geçersiz {sectionPath} yapılandırması.", ex);
            }
        }
    }
}
