// File: src/ArchiX.Library/Infrastructure/Http/IHttpClientWrapper.cs
namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// Dış servislerle haberleşmeyi soyutlayan temel HttpClient wrapper arabirimi.
    /// Amaç: tek noktadan timeout, retry, logging, correlation ve hata eşleme politikalarını uygulamak.
    /// Implementasyon tipik olarak named/typed HttpClient ve DelegatingHandler zinciri ile sağlanır.
    /// </summary>
    public interface IHttpClientWrapper
    {
        /// <summary>
        /// Ham <see cref="HttpRequestMessage"/> gönderir. İleri seviye senaryolar için esnek noktadır.
        /// Handler/Policy zincirleri (retry, logging, correlation vs.) bu çağrıda devreye girer.
        /// </summary>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);

        /// <summary>
        /// GET isteği atar ve gövdeyi düz metin olarak döner.
        /// 4xx ve 5xx durumlarında uygun istisna atılacağı varsayılır (implementasyonda).
        /// </summary>
        Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// GET isteği atar ve yanıt gövdesini JSON'dan T tipine deseriyalize ederek döner.
        /// </summary>
        Task<T?> GetJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// JSON gövdeli POST isteği atar ve yanıtı TResponse tipine deseriyalize ederek döner.
        /// </summary>
        Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default);

        /// <summary>
        /// JSON gövdeli PUT isteği atar ve yanıtı TResponse tipine deseriyalize ederek döner.
        /// </summary>
        Task<TResponse?> PutJsonAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default);

        /// <summary>
        /// DELETE isteği atar. Gövdesiz 2xx yanıtları için yeterlidir.
        /// </summary>
        Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
    }
}
