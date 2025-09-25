// File: src/ArchiX.Library/External/PingAdapter.cs
#nullable enable
using ArchiX.Library.Infrastructure.Http; // ProblemDetailsReader, HttpApiProblem

using Microsoft.Extensions.Logging;

namespace ArchiX.Library.External
{
    /// <summary>Basit dış servis ping adapteri. GET /status çağırır.</summary>
    /// <remarks>Başarısız yanıtlarda RFC7807 ProblemDetails okumayı dener.</remarks>
    /// <param name="http">Named/typed HttpClient. BaseAddress ve header’lar DI aşamasında ayarlanır.</param>
    /// <param name="log">Adapter için logger.</param>
    public sealed class PingAdapter(HttpClient http, ILogger<PingAdapter> log) : IPingAdapter
    {
        private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
        private readonly ILogger<PingAdapter> _log = log ?? throw new ArgumentNullException(nameof(log));

        /// <summary>/status çağrısı. 2KB üstü gövde kesilir.</summary>
        /// <param name="ct">İptal belirteci.</param>
        /// <returns>Yanıt gövdesi (text).</returns>
        /// <exception cref="HttpRequestException">Başarısız HTTP durumlarında fırlatılır.</exception>
        public async Task<string> GetStatusTextAsync(CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "status");
            using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct)
                                       .ConfigureAwait(false);

            if (res.IsSuccessStatusCode)
            {
                var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return text.Length <= 2048 ? text : text[..2048];
            }

            // Hata: ProblemDetails dene, değilse düz metin
            var problem = await ProblemDetailsReader.TryReadAsync(res, ct).ConfigureAwait(false);
            var detail = problem is null
                ? await SafeReadTextAsync(res, ct).ConfigureAwait(false)
                : ProblemDetailsReader.ToOneLine(problem);

            var msg = $"Ping /status hata: {(int)res.StatusCode} {res.ReasonPhrase}. " +
                      (string.IsNullOrWhiteSpace(detail) ? "Gövde yok." : detail);

            _log.LogWarning("{Message}", msg);
            throw new HttpRequestException(msg, null, res.StatusCode);
        }

        /// <summary>Gövdeyi güvenle okur ve 2KB ile sınırlar.</summary>
        private static async Task<string> SafeReadTextAsync(HttpResponseMessage res, CancellationToken ct)
        {
            try
            {
                var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(text)
                    ? ""
                    : (text.Length <= 2048 ? text : text[..2048]);
            }
            catch
            {
                return "";
            }
        }
    }
}
