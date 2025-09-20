// File: src/ArchiX.Library/Infrastructure/Http/HttpClientWrapperBase.cs
using System.Text;
using System.Text.Json;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// <see cref="IHttpClientWrapper"/> için temel implementasyon.
    /// Ortak JSON serileştirme, istek gönderme ve hata eşleme mantığını içerir.
    /// </summary>
    public abstract class HttpClientWrapperBase : IHttpClientWrapper
    {
        /// <summary>HTTP çağrıları için kullanılan <see cref="HttpClient"/> örneği.</summary>
        protected readonly HttpClient Client;

        /// <summary>JSON serileştirme seçenekleri.</summary>
        protected readonly JsonSerializerOptions JsonOptions;

        /// <summary>Yeni bir <see cref="HttpClientWrapperBase"/> örneği oluşturur.</summary>
        /// <param name="client">Kullanılacak <see cref="HttpClient"/>.</param>
        /// <param name="jsonOptions">JSON serileştirme seçenekleri. Boş ise varsayılanlar kullanılır.</param>
        protected HttpClientWrapperBase(HttpClient client, JsonSerializerOptions? jsonOptions = null)
        {
            Client = client;
            JsonOptions = jsonOptions ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>Verilen HTTP isteğini gönderir.</summary>
        public virtual async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }

        /// <summary>GET isteği gönderir ve yanıt gövdesini düz metin olarak döner.</summary>
        public virtual async Task<string> GetStringAsync(
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>GET isteği gönderir ve yanıtı JSON'dan belirtilen tipe deseriyalize ederek döner.</summary>
        public virtual async Task<T?> GetJsonAsync<T>(
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            var json = await GetStringAsync(requestUri, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        /// <summary>JSON gövdeli POST isteği gönderir ve yanıtı belirtilen tipe deseriyalize ederek döner.</summary>
        public virtual async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(
            string requestUri,
            TRequest body,
            CancellationToken cancellationToken = default)
        {
            using var content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength == 0)
                return default;

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<TResponse>(json, JsonOptions);
        }

        /// <summary>JSON gövdeli PUT isteği gönderir ve yanıtı belirtilen tipe deseriyalize ederek döner.</summary>
        public virtual async Task<TResponse?> PutJsonAsync<TRequest, TResponse>(
            string requestUri,
            TRequest body,
            CancellationToken cancellationToken = default)
        {
            using var content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content };
            using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength == 0)
                return default;

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<TResponse>(json, JsonOptions);
        }

        /// <summary>DELETE isteği gönderir.</summary>
        public virtual async Task<HttpResponseMessage> DeleteAsync(
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }
    }
}
