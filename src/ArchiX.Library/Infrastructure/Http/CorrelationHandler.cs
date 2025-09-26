// File: src/ArchiX.Library/Infrastructure/Http/CorrelationHandler.cs
#nullable enable
namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>Giden isteklere standart korelasyon başlığını ekler, yanıta geri yazar.</summary>
    /// <remarks>
    /// Başlık adı <see cref="HttpCorrelation.HeaderName"/>. İstek üzerinde yoksa yeni kimlik üretir.
    /// Yanıt üstbilgisine aynı kimliği yazar.
    /// </remarks>
    public sealed class CorrelationHandler() : DelegatingHandler
    {
        /// <summary>İstek/yanıt akışında korelasyon başlığını garanti eder.</summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var id = HttpCorrelation.EnsureOnRequest(request);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            HttpCorrelation.SetOnResponse(response, id);
            return response;
        }
    }
}
