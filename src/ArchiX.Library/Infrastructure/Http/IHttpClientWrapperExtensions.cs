// File: src/ArchiX.Library/Infrastructure/Http/IHttpClientWrapperExtensions.cs
using System.Text;
using System.Text.Json;
using ArchiX.Library.Abstractions.Http;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// <see cref="IHttpClientWrapper"/> için yardımcı HTTP uzantıları (HEAD, PATCH ve genel JSON gönderimi).
    /// </summary>
    public static class IHttpClientWrapperExtensions
    {
        private static readonly HttpMethod PatchMethod = new("PATCH");

        /// <summary>
        /// HEAD isteği gönderir (gövdesiz). Başlıklara hızlı bakış için idealdir.
        /// </summary>
        /// <param name="wrapper">HTTP istemci sarmalayıcı.</param>
        /// <param name="requestUri">İstek göreli/ mutlak yolu.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>HTTP yanıtı.</returns>
        public static async Task<HttpResponseMessage> HeadAsync(
            this IHttpClientWrapper wrapper,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(wrapper);

            using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
            var response = await wrapper.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// JSON gövdeli PATCH isteği gönderir ve yanıtı TResponse tipine deseriyalize ederek döner.
        /// </summary>
        /// <typeparam name="TRequest">Gönderilecek gövde tipi.</typeparam>
        /// <typeparam name="TResponse">Yanıtın deseriyalize edileceği tip.</typeparam>
        /// <param name="wrapper">HTTP istemci sarmalayıcı.</param>
        /// <param name="requestUri">İstek göreli/ mutlak yolu.</param>
        /// <param name="body">JSON olarak gönderilecek gövde.</param>
        /// <param name="jsonOptions">Opsiyonel JSON seçenekleri (boşsa Web varsayılanları kullanılır).</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        public static Task<TResponse?> PatchJsonAsync<TRequest, TResponse>(
            this IHttpClientWrapper wrapper,
            string requestUri,
            TRequest body,
            JsonSerializerOptions? jsonOptions = null,
            CancellationToken cancellationToken = default)
            => SendJsonAsync<TRequest, TResponse>(wrapper, PatchMethod, requestUri, body, jsonOptions, cancellationToken);

        /// <summary>
        /// JSON gövdeli istek gönderir (POST/PUT/PATCH gibi) ve yanıtı TResponse tipine deseriyalize ederek döner.
        /// </summary>
        /// <typeparam name="TRequest">Gönderilecek gövde tipi.</typeparam>
        /// <typeparam name="TResponse">Yanıtın deseriyalize edileceği tip.</typeparam>
        /// <param name="wrapper">HTTP istemci sarmalayıcı.</param>
        /// <param name="method">HTTP metodu (POST/PUT/PATCH...).</param>
        /// <param name="requestUri">İstek göreli/ mutlak yolu.</param>
        /// <param name="body">JSON olarak gönderilecek gövde.</param>
        /// <param name="jsonOptions">Opsiyonel JSON seçenekleri (boşsa Web varsayılanları kullanılır).</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>TResponse veya içerik yoksa <c>default</c>.</returns>
        public static async Task<TResponse?> SendJsonAsync<TRequest, TResponse>(
            this IHttpClientWrapper wrapper,
            HttpMethod method,
            string requestUri,
            TRequest body,
            JsonSerializerOptions? jsonOptions = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(wrapper);
            ArgumentNullException.ThrowIfNull(method);

            var opts = jsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };

            using var content = new StringContent(JsonSerializer.Serialize(body, opts), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(method, requestUri) { Content = content };
            using var response = await wrapper.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var respContent = response.Content;
            if (respContent is null)
                return default;

            if (respContent.Headers?.ContentLength is long len && len == 0)
                return default;

            var json = await respContent.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<TResponse>(json, opts);
        }
    }
}
