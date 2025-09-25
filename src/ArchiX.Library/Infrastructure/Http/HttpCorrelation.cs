// File: src/ArchiX.Library/Infrastructure/Http/HttpCorrelation.cs
#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>HTTP korelasyon kimliği yardımcıları.</summary>
    public static class HttpCorrelation
    {
        /// <summary>Standart korelasyon başlığı adı.</summary>
        public const string HeaderName = "X-Correlation-ID";

        /// <summary>Yeni korelasyon kimliği üretir (32 haneli, tire yok).</summary>
        public static string NewId() => Guid.NewGuid().ToString("N");

        /// <summary>İstek üstbilgisinde korelasyon kimliğini garanti eder ve döner.</summary>
        public static string EnsureOnRequest(HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (TryGetFromRequest(request, out var id) && !string.IsNullOrWhiteSpace(id))
                return id!;
            id = NewId();
            request.Headers.Remove(HeaderName);
            request.Headers.TryAddWithoutValidation(HeaderName, id);
            return id;
        }

        /// <summary>İstekten korelasyon kimliğini okur.</summary>
        public static bool TryGetFromRequest(HttpRequestMessage request, [NotNullWhen(true)] out string? id)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.Headers.TryGetValues(HeaderName, out var vals))
            {
                var v = vals is null ? null : string.Join(",", vals);
                if (!string.IsNullOrWhiteSpace(v)) { id = v; return true; }
            }
            id = null;
            return false;
        }

        /// <summary>Yanıta korelasyon kimliği yazar.</summary>
        public static void SetOnResponse(HttpResponseMessage response, string id)
        {
            ArgumentNullException.ThrowIfNull(response);
            if (string.IsNullOrWhiteSpace(id)) return;
            response.Headers.Remove(HeaderName);
            response.Headers.TryAddWithoutValidation(HeaderName, id);
        }

        /// <summary>Yanttan korelasyon kimliğini okur.</summary>
        public static bool TryGetFromResponse(HttpResponseMessage response, [NotNullWhen(true)] out string? id)
        {
            ArgumentNullException.ThrowIfNull(response);
            if (response.Headers.TryGetValues(HeaderName, out var vals))
            {
                var v = vals is null ? null : string.Join(",", vals);
                if (!string.IsNullOrWhiteSpace(v)) { id = v; return true; }
            }
            id = null;
            return false;
        }
    }
}
