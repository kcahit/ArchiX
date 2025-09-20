// File: src/ArchiX.Library/Infrastructure/Http/CorrelationHandler.cs
namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// Her HTTP isteğine X-Correlation-ID ekleyen <see cref="DelegatingHandler"/>.
    /// Kimlik zaten varsa dokunmaz; yoksa yeni bir GUID üretir.
    /// </summary>
    public sealed class CorrelationHandler : DelegatingHandler
    {
        private const string HeaderName = "X-Correlation-ID";

        /// <summary>İsteğe korelasyon kimliği ekler ve zinciri devam ettirir.</summary>
        /// <param name="request">HTTP isteği.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>HTTP yanıtı.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(HeaderName))
            {
                var corrId = Guid.NewGuid().ToString("D");
                request.Headers.Add(HeaderName, corrId);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
