// File: src/ArchiX.Library/External/PingAdapterServiceCollectionExtensions.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

using ArchiX.Library.Infrastructure.Http; // CorrelationHandler, OutboundLoggingHandler, RetryHandler, TimeoutHandler, ProblemDetailsHandler, HttpPoliciesOptions

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.External
{
    /// <summary>Ping adapter DI kayıt uzantıları.</summary>
    public static class PingAdapterServiceCollectionExtensions
    {
        /// <summary>
        /// <see cref="IPingAdapter"/>’ı tipli HttpClient ile kaydeder.
        /// Correlation → OutboundLogging → Retry → Timeout → ProblemDetails zinciri bağlanır.
        /// Retry/Timeout değerleri varsa parametrelerden, yoksa <see cref="HttpPoliciesOptions"/>’tan alınır.
        /// </summary>
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
            services.AddTransient<ProblemDetailsHandler>();

            // Retry/Timeout handler'larını options'a göre kur
            services.AddHttpClient<IPingAdapter, PingAdapter>(client =>
            {
                client.BaseAddress = baseAddress;
                if (timeout is { } t) client.Timeout = t;
                client.DefaultRequestHeaders.Accept.ParseAdd("text/plain");
            })
                .AddHttpMessageHandler<CorrelationHandler>()
                .AddHttpMessageHandler<OutboundLoggingHandler>()
                .AddHttpMessageHandler(sp =>
                {
                    var opts = sp.GetService<IOptions<HttpPoliciesOptions>>()?.Value;
                    return new RetryHandler(
                        maxRetries: opts?.RetryCount ?? 3,
                        baseDelay: opts?.GetBaseDelay() ?? TimeSpan.FromMilliseconds(200));
                })
                .AddHttpMessageHandler(sp =>
                {
                    var opts = sp.GetService<IOptions<HttpPoliciesOptions>>()?.Value;
                    var effectiveTimeout = timeout ?? opts?.GetTimeout() ?? TimeSpan.FromSeconds(100);
                    return new TimeoutHandler(effectiveTimeout);
                })
                .AddHttpMessageHandler<ProblemDetailsHandler>();

            return services;
        }

        /// <summary>
        /// Konfigürasyondan <see cref="IPingAdapter"/> kaydı yapar.
        /// Varsayılan bölüm: <c>ExternalServices:Ping</c>.
        /// <list type="bullet">
        /// <item><description><c>BaseAddress</c> (zorunlu, URL)</description></item>
        /// <item><description><c>TimeoutSeconds</c> (opsiyonel, 1–300)</description></item>
        /// </list>
        /// </summary>
        public static IServiceCollection AddPingAdapter(
            this IServiceCollection services,
            IConfiguration config,
            string sectionPath = "ExternalServices:Ping")
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
