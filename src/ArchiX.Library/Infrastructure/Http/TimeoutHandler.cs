// File: src/ArchiX.Library/Infrastructure/Http/TimeoutHandler.cs
namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// İstek bazlı zaman aşımı (timeout) uygulayan delegating handler.
    /// Varsayılan süreyi ctor'dan alır; her istek için <see cref="TimeoutKey"/> ile override edilebilir.
    /// </summary>
    public sealed class TimeoutHandler : DelegatingHandler
    {
        /// <summary>
        /// Bir isteğe özel timeout süresi geçmek için kullanılacak <see cref="HttpRequestOptionsKey{TValue}"/>.
        /// Örnek: <c>request.Options.Set(TimeoutHandler.TimeoutKey, TimeSpan.FromSeconds(5));</c>
        /// </summary>
        public static readonly HttpRequestOptionsKey<TimeSpan> TimeoutKey = new("ArchiX.Timeout");

        private readonly TimeSpan _defaultTimeout;

        /// <summary>
        /// Yeni bir <see cref="TimeoutHandler"/> oluşturur.
        /// </summary>
        /// <param name="defaultTimeout">Varsayılan zaman aşımı. Belirtilmezse 100 saniye.</param>
        public TimeoutHandler(TimeSpan? defaultTimeout = null)
        {
            _defaultTimeout = defaultTimeout ?? TimeSpan.FromSeconds(100);
            if (_defaultTimeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(defaultTimeout), "Timeout sıfırdan büyük olmalıdır.");
        }

        /// <summary>
        /// İsteği belirtilen süre dolmadan tamamlamaya zorlar.
        /// Eğer istek üzerinde <see cref="TimeoutKey"/> ayarlıysa onu kullanır; değilse varsayılanı.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // İstek özelinde süre var mı?
            var effectiveTimeout = _defaultTimeout;
            if (request.Options.TryGetValue(TimeoutKey, out var perRequest) && perRequest > TimeSpan.Zero)
                effectiveTimeout = perRequest;

            // HttpClient.Timeout yerine token tabanlı iptal uyguluyoruz.
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(effectiveTimeout);

            try
            {
                return await base.SendAsync(request, cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Dış iptal değilse, gerçek bir timeout yaşandı.
                throw new TimeoutException($"HTTP isteği zaman aşımına uğradı. Süre: {effectiveTimeout}.");
            }
        }
    }
}
