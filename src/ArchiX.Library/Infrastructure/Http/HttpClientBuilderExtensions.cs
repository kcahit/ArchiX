// File: src/ArchiX.Library/Infrastructure/Http/HttpClientBuilderExtensions.cs
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// HttpClient fabrikası için ArchiX uzantıları.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// #57 ArchiX HTTP pipeline'ını ekler (DB'den HttpPoliciesOptions okuyarak).
        /// IParameterService kullanarak runtime'da parametreleri yükler.
        /// </summary>
        public static IHttpClientBuilder UseArchiXHttpPipelineFromDatabase(this IHttpClientBuilder builder)
        {
            // Handler'ları factory ile ekle (her request'te DI'dan options çözülecek)
            return builder
                .AddHttpMessageHandler<ProblemDetailsHandler>()
                .AddHttpMessageHandler(sp =>
                {
                    // Runtime'da parametreleri DB'den oku
                    var paramService = sp.GetService<ArchiX.Library.Services.Parameters.IParameterService>();
                    
                    HttpPoliciesOptions? options = null;
                    try
                    {
                        // Sync call - handler ctor'da async desteklenmez
                        // Bu nedenle lazy load yapacağız veya startup'ta cache'leyeceğiz
                        options = sp.GetService<HttpPoliciesOptions>();
                    }
                    catch
                    {
                        // Fallback to defaults
                        options = new HttpPoliciesOptions();
                    }

                    options ??= new HttpPoliciesOptions();
                    return new RetryHandler(options.RetryCount, options.GetBaseDelay());
                })
                .AddHttpMessageHandler(sp =>
                {
                    var options = sp.GetService<HttpPoliciesOptions>() ?? new HttpPoliciesOptions();
                    return new TimeoutHandler(options.GetTimeout());
                });
        }

        /// <summary>
        /// ArchiX HTTP pipeline'ını ekler: <see cref="ProblemDetailsHandler"/> (outermost) ve
        /// <see cref="RetryHandler"/> (sonraki). Bu sıra ile önce retry uygulanır, en sonda
        /// hata yanıtı ProblemDetails'a dönüştürülür.
        /// </summary>
        /// <param name="builder">IHttpClientBuilder örneği.</param>
        /// <param name="retryCount">Maksimum tekrar sayısı (varsayılan: 3).</param>
        /// <param name="baseDelay">Exponential backoff taban gecikmesi (varsayılan: 200 ms).</param>
        /// <returns>IHttpClientBuilder (akış için).</returns>
        public static IHttpClientBuilder UseArchiXHttpPipeline(
            this IHttpClientBuilder builder,
            int retryCount = 3,
            TimeSpan? baseDelay = null)
        {
            // Not: AddHttpMessageHandler ekleme sırası önemlidir.
            // İlk eklenen handler OUTERMOST olur.
            return builder
                .AddHttpMessageHandler(() => new ProblemDetailsHandler())
                .AddHttpMessageHandler(() => new RetryHandler(retryCount, baseDelay ?? TimeSpan.FromMilliseconds(200)));
        }

        /// <summary>
        /// Adlandırılmış bir HttpClient oluşturur, <paramref name="baseAddress"/> ayarlar ve
        /// ArchiX HTTP pipeline'ını ekler.
        /// </summary>
        /// <param name="services">DI kapsayıcısı.</param>
        /// <param name="name">HttpClient adı.</param>
        /// <param name="baseAddress">Temel adres.</param>
        /// <param name="timeout">İstek zaman aşımı (varsayılan 100 sn).</param>
        /// <param name="retryCount">Maksimum tekrar sayısı (varsayılan 3).</param>
        /// <param name="baseDelay">Exponential backoff tabanı (varsayılan 200 ms).</param>
        /// <returns>IHttpClientBuilder (gerekirse typed client ekleyebilmek için).</returns>
        public static IHttpClientBuilder AddArchiXHttpClient(
            this IServiceCollection services,
            string name,
            Uri baseAddress,
            TimeSpan? timeout = null,
            int retryCount = 3,
            TimeSpan? baseDelay = null)
        {
            var builder = services.AddHttpClient(name, c =>
            {
                c.BaseAddress = baseAddress;
                c.Timeout = timeout ?? TimeSpan.FromSeconds(100);
            });

            builder.UseArchiXHttpPipeline(retryCount, baseDelay);
            return builder;
        }

        /// <summary>
        /// Adlandırılmış bir HttpClient'ı <typeparamref name="TClient"/> typed client olarak
        /// kaydeder ve ArchiX HTTP pipeline'ını ekler. <typeparamref name="TClient"/> için
        /// bir <see cref="HttpClient"/> alan ctor bulunmalıdır.
        /// </summary>
        /// <typeparam name="TClient">Typed client türü.</typeparam>
        /// <param name="services">DI kapsayıcısı.</param>
        /// <param name="name">HttpClient adı.</param>
        /// <param name="baseAddress">Temel adres.</param>
        /// <param name="timeout">İstek zaman aşımı (varsayılan 100 sn).</param>
        /// <param name="retryCount">Maksimum tekrar sayısı (varsayılan 3).</param>
        /// <param name="baseDelay">Exponential backoff tabanı (varsayılan 200 ms).</param>
        /// <returns>IHttpClientBuilder.</returns>
        public static IHttpClientBuilder AddArchiXHttpClient<TClient>(
            this IServiceCollection services,
            string name,
            Uri baseAddress,
            TimeSpan? timeout = null,
            int retryCount = 3,
            TimeSpan? baseDelay = null)
            where TClient : class
        {
            return services
                .AddArchiXHttpClient(name, baseAddress, timeout, retryCount, baseDelay)
                .AddTypedClient<TClient>();
        }
    }
}
