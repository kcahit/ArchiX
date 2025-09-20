// File: src/ArchiX.Library/Infrastructure/Http/OutboundLoggingHandler.cs
using System.Diagnostics;
using System.Net.Http.Headers;

using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// Dışa giden HTTP istek/yanıtlarını güvenli ve hafif biçimde loglar.
    /// Gövde okunmaz; yalnızca meta veriler (metot, uri, durum, süre, content-length vb.) yazılır.
    /// </summary>
    public sealed class OutboundLoggingHandler(ILogger<OutboundLoggingHandler> logger) : DelegatingHandler
    {
        private readonly ILogger<OutboundLoggingHandler> _logger =
            logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// İsteği gönderir, süreyi ölçer ve yanıtla birlikte loglar.
        /// Hata oluşursa hatayı da süre bilgisiyle birlikte loglar ve yeniden fırlatır.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var sw = Stopwatch.StartNew();
            var method = request.Method.Method;
            var uri = request.RequestUri?.ToString() ?? "(null)";

            // Korelasyon kimliği (istek veya yanıt header’ından gelebilir)
            var correlationId = TryGetHeader(request.Headers, "X-Correlation-ID");

            // İçerik uzunluğu (gövde okunmaz)
            long? reqLen = request.Content?.Headers?.ContentLength;

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                sw.Stop();

                // Yanıt bilgileri
                var status = (int)response.StatusCode;
                var reason = response.ReasonPhrase;
                correlationId ??= TryGetHeader(response.Headers, "X-Correlation-ID");
                long? respLen = response.Content?.Headers?.ContentLength;

                // Yapısal log
                _logger.LogInformation(
                    "HTTP OUT {Method} {Uri} -> {Status} {Reason} | dur={DurationMs}ms | corr={CorrelationId} | reqLen={ReqLen} | respLen={RespLen}",
                    method, uri, status, reason, sw.ElapsedMilliseconds, correlationId, reqLen, respLen);

                // İsteğe bağlı: header isimlerini (değerleri değil) yaz
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    var reqHeaderNames = string.Join(",", request.Headers.Select(h => h.Key));
                    var respHeaderNames = string.Join(",", response.Headers.Select(h => h.Key));
                    _logger.LogDebug("HTTP OUT headers -> req[{ReqHeaders}] resp[{RespHeaders}]", reqHeaderNames, respHeaderNames);
                }

                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "HTTP OUT FAILED {Method} {Uri} | dur={DurationMs}ms | corr={CorrelationId}",
                    method, uri, sw.ElapsedMilliseconds, correlationId);
                throw;
            }
        }

        /// <summary>
        /// Header’lar içinde adı verilen başlığın ilk değerini döndürür; yoksa null.
        /// </summary>
        private static string? TryGetHeader(HttpHeaders headers, string name)
        {
            if (headers.TryGetValues(name, out var vals))
                return vals.FirstOrDefault();
            return null;
        }
    }
}
