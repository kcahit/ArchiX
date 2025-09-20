// File: src/ArchiX.Library/Infrastructure/Http/RetryHandler.cs
using System.Net;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// Geçici hatalarda (5xx, 408, 429 vb.) retry uygulayan DelegatingHandler.
    /// Exponential backoff + Retry-After desteği. Gövdeli isteklerde içerik buffer'lanır ve
    /// her denemede yeniden oluşturulur; böylece POST/PUT/PATCH gövdesi kaybolmaz.
    /// </summary>
    public sealed class RetryHandler : DelegatingHandler
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _baseDelay;

        /// <summary>
        /// Yeni bir <see cref="RetryHandler"/> oluşturur.
        /// </summary>
        /// <param name="maxRetries">Maksimum tekrar sayısı (0 = retry yok).</param>
        /// <param name="baseDelay">İlk gecikme süresi (exponential backoff için taban).</param>
        public RetryHandler(int maxRetries = 3, TimeSpan? baseDelay = null)
        {
            if (maxRetries < 0) ThrowOutOfRange(nameof(maxRetries));
            if (baseDelay is { } d && d < TimeSpan.Zero) ThrowOutOfRange(nameof(baseDelay));

            _maxRetries = maxRetries;
            _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
        }

        /// <summary>
        /// İsteği gönderir; geçici hatalarda belirtilen stratejiyle yeniden dener.
        /// Gövdeli istekler için içerik buffer'lanır ve her denemede yeniden ayarlanır.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // 1) İçeriği tek sefer buffer'la (küçük/orta boy JSON gövdeleri hedeflenmiştir)
            var (bytes, headers) = await BufferContentAsync(request.Content, cancellationToken).ConfigureAwait(false);
            if (bytes is not null)
                request.Content = CreateContent(bytes, headers);

            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                lastException = null;

                try
                {
                    response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (!ShouldRetry(response))
                        return response;
                }
                catch (HttpRequestException ex) when (IsTransient(ex))
                {
                    lastException = ex;
                }
                catch (TaskCanceledException ex)
                {
                    // Dış iptal (cancellationToken) istendiyse yükselt.
                    if (cancellationToken.IsCancellationRequested) throw;
                    lastException = ex; // timeout say
                }

                // Buraya geldiysek retry edeceğiz.
                response?.Dispose();

                if (attempt == _maxRetries)
                    break;

                var delay = ComputeDelay(response, attempt);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                // Eski isteği at, yenisini oluştur ve header/opsiyonları taşı.
                var old = request;
                request = CloneRequest(old);
                old.Dispose();

                if (bytes is not null)
                    request.Content = CreateContent(bytes, headers);
            }

            if (lastException is not null) throw lastException;
            throw new HttpRequestException("Request failed after retries.");
        }

        /// <summary>
        /// 5xx, 408, 429 için retry uygula. Diğer durumlarda geç.
        /// </summary>
        private static bool ShouldRetry(HttpResponseMessage response)
        {
            var code = (int)response.StatusCode;
            return code >= 500 ||
                   response.StatusCode == HttpStatusCode.RequestTimeout ||   // 408
                   response.StatusCode == (HttpStatusCode)429;              // Too Many Requests
        }

        /// <summary>
        /// Retry bekleme süresini hesapla. 429/503 için Retry-After'ı dikkate al;
        /// yoksa exponential backoff: baseDelay * 2^attempt.
        /// </summary>
        private TimeSpan ComputeDelay(HttpResponseMessage? response, int attempt)
        {
            if (response?.Headers?.RetryAfter is { } ra)
            {
                if (ra.Delta is TimeSpan delta && delta > TimeSpan.Zero)
                    return delta;
                if (ra.Date is DateTimeOffset when)
                {
                    var diff = when - DateTimeOffset.UtcNow;
                    if (diff > TimeSpan.Zero) return diff;
                }
            }

            var ms = _baseDelay.TotalMilliseconds * Math.Pow(2, attempt);
            return TimeSpan.FromMilliseconds(Math.Max(0, ms));
        }

        /// <summary>
        /// HttpRequestMessage'ı güvenli şekilde klonla (Content hariç).
        /// </summary>
        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
                VersionPolicy = request.VersionPolicy
            };

            // Headers
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

#if NET5_0_OR_GREATER
            // Options: string adları HttpRequestOptionsKey<T> ile sar.
            foreach (var pair in request.Options)
            {
                var keyName = pair.Key;           // string
                var value = pair.Value;           // object?
                var keyObj = new HttpRequestOptionsKey<object?>(keyName);
                clone.Options.Set(keyObj, value);
            }
#endif
            return clone;
        }

        /// <summary>
        /// İçeriği bayt dizisine buffer'lar ve header'ları kopyalar.
        /// Büyük gövdeler için önerilen yaklaşım değildir (streaming yerine JSON/küçük içerikler hedeflenir).
        /// </summary>
        private static async Task<(byte[]? bytes, Dictionary<string, IEnumerable<string>>? headers)>
            BufferContentAsync(HttpContent? content, CancellationToken ct)
        {
            if (content is null) return (null, null);

            // header'ları al
            var headers = content.Headers.ToDictionary(h => h.Key, h => h.Value, StringComparer.OrdinalIgnoreCase);

            // içeriği buffer'la
            var bytes = await content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            return (bytes, headers);
        }

        /// <summary>
        /// Buffer'lanmış gövde ile yeni HttpContent üret ve header'ları uygula.
        /// </summary>
        private static ByteArrayContent CreateContent(byte[] bytes, Dictionary<string, IEnumerable<string>>? headers)
        {
            var newContent = new ByteArrayContent(bytes);
            if (headers is not null)
            {
                foreach (var kv in headers)
                    newContent.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }
            return newContent;
        }

        // CA1512: throw helper
        private static void ThrowOutOfRange(string paramName) =>
            throw new ArgumentOutOfRangeException(paramName);

        // Geçici ağ hataları için koruyucu (geniş tutuldu)
        private static bool IsTransient(HttpRequestException _) => true;
    }
}
