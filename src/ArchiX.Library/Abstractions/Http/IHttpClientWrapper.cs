namespace ArchiX.Library.Abstractions.Http;

public interface IHttpClientWrapper
{
 Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
 Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default);
 Task<T?> GetJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default);
 Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default);
 Task<TResponse?> PutJsonAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default);
 Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}
