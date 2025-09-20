// File: src/ArchiX.Library/Infrastructure/Http/ProblemDetailsHandler.cs
using System.Text;
using System.Text.Json;

namespace ArchiX.Library.Infrastructure.Http
{
    /// <summary>
    /// 4xx/5xx yanıtlarda RFC 7807 (application/problem+json) gövdesini parse ederek
    /// anlamlı bir <see cref="ProblemDetailsException"/> fırlatan delegating handler.
    /// Başarılı (2xx) yanıtları olduğu gibi geçirir.
    /// </summary>
    public sealed class ProblemDetailsHandler : DelegatingHandler
    {
        /// <summary>ProblemDetails ayrıştırması için JSON seçenekleri.</summary>
        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return response;

            // CorrelationId: header varsa ilk değeri al
            string? correlationId = response.Headers.TryGetValues("X-Correlation-ID", out var vals)
                ? vals.FirstOrDefault()
                : null;

            // ⚙️ IDE0059 fix: raw kullanılmadığı için discard kullanıyoruz
            var (payload, _) = await ReadProblemDetailsAsync(response, cancellationToken).ConfigureAwait(false);

            // Özel exception fırlat
            throw new ProblemDetailsException(
                status: (int)response.StatusCode,
                method: request.Method.Method,
                uri: request.RequestUri?.ToString() ?? "(null)",
                correlationId: correlationId,
                title: payload?.Title,
                detail: payload?.Detail,
                type: payload?.Type,
                instance: payload?.Instance,
                errors: payload?.Errors
            );
        }

        /// <summary>
        /// Yanıttan ProblemDetails (varsa) ve ham gövdeyi okur.
        /// ProblemDetails yoksa (null, null) döner.
        /// </summary>
        private static async Task<(ProblemDetailsPayload? payload, string? raw)>
            ReadProblemDetailsAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.Content is null)
                return (null, null);

            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(body))
                return (null, null);

            var mediaType = response.Content.Headers?.ContentType?.MediaType;
            if (!string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase))
                return (null, body);

            try
            {
                var parsed = JsonSerializer.Deserialize<ProblemDetailsPayload>(body, JsonOptions);
                return (parsed, body);
            }
            catch
            {
                // Parse edilemezse raw gövdeyle dön.
                return (null, body);
            }
        }

        /// <summary>
        /// Exception mesajı için kısa, okunabilir metin oluşturur.
        /// ProblemDetails yoksa raw gövdenin ilk 512 karakterini ekler.
        /// </summary>
        private static string BuildMessage(
            HttpResponseMessage response,
            ProblemDetailsPayload? payload,
            string? raw,
            string? correlationId)
        {
            var sb = new StringBuilder();
            sb.Append("HTTP ").Append((int)response.StatusCode).Append(' ').Append(response.ReasonPhrase);

            if (!string.IsNullOrWhiteSpace(payload?.Title))
                sb.Append(" | ").Append(payload!.Title);

            if (!string.IsNullOrWhiteSpace(payload?.Detail))
                sb.Append(" | ").Append(payload!.Detail);

            if (!string.IsNullOrWhiteSpace(correlationId))
                sb.Append(" | Correlation: ").Append(correlationId);

            if (payload is null && !string.IsNullOrWhiteSpace(raw))
            {
                // CA1845/IDE0057: Substring yerine AsSpan + string.Concat kullan
                var snippet = raw!.Length > 512
                    ? string.Concat(raw.AsSpan(0, 512), "…")
                    : raw;
                sb.Append(" | Body: ").Append(snippet);
            }

            return sb.ToString();
        }

        /// <summary>
        /// ASP.NET bağımlılığı olmadan minimal ProblemDetails modeli.
        /// Validation hataları için yaygın "errors" sözlüğünü destekler.
        /// </summary>
        private sealed class ProblemDetailsPayload
        {
            /// <summary>Hata türünü tarif eden URI.</summary>
            public string? Type { get; set; }

            /// <summary>Kısa başlık.</summary>
            public string? Title { get; set; }

            /// <summary>Ayrıntılı açıklama.</summary>
            public string? Detail { get; set; }

            /// <summary>HTTP durum kodu.</summary>
            public int? Status { get; set; }

            /// <summary>İstisnai olayın örnek URI'si.</summary>
            public string? Instance { get; set; }

            /// <summary>Alan bazlı doğrulama hataları.</summary>
            public Dictionary<string, string[]>? Errors { get; set; }
        }
    }

    /// <summary>
    /// Dış servislerden dönen RFC 7807 ProblemDetails yükünü temsil eden istisna.
    /// </summary>
    public sealed class ProblemDetailsException : HttpRequestException
    {
        /// <summary>HTTP durum kodu (int).</summary>
        public int Status { get; }

        /// <summary>HTTP metodu (GET/POST/...)</summary>
        public string Method { get; }

        /// <summary>İstek URI'si.</summary>
        public string Uri { get; }

        /// <summary>Sunucu tarafından üretilmiş korelasyon kimliği.</summary>
        public string? CorrelationId { get; }

        /// <summary>ProblemDetails başlığı.</summary>
        public string? Title { get; }

        /// <summary>ProblemDetails detay açıklaması.</summary>
        public string? Detail { get; }

        /// <summary>Problem türü URI'si.</summary>
        public string? Type { get; }

        /// <summary>Problem örneği URI'si.</summary>
        public string? Instance { get; }

        /// <summary>Doğrulama hataları sözlüğü.</summary>
        public IReadOnlyDictionary<string, string[]>? Errors { get; }

        /// <summary>
        /// Yeni bir <see cref="ProblemDetailsException"/> oluşturur.
        /// </summary>
        public ProblemDetailsException(
            int status,
            string method,
            string uri,
            string? correlationId,
            string? title,
            string? detail,
            string? type,
            string? instance,
            Dictionary<string, string[]>? errors)
            : base(BuildMessage(status, method, uri, correlationId, title, detail))
        {
            Status = status;
            Method = method;
            Uri = uri;
            CorrelationId = correlationId;
            Title = title;
            Detail = detail;
            Type = type;
            Instance = instance;
            Errors = errors ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            // Tanı kolaylığı için bilgi ekleri
            Data[nameof(Status)] = status;
            Data[nameof(Method)] = method;
            Data[nameof(Uri)] = uri;
            if (!string.IsNullOrWhiteSpace(correlationId)) Data[nameof(CorrelationId)] = correlationId;
            if (!string.IsNullOrWhiteSpace(title)) Data[nameof(Title)] = title;
            if (!string.IsNullOrWhiteSpace(detail)) Data[nameof(Detail)] = detail;
            if (!string.IsNullOrWhiteSpace(type)) Data[nameof(Type)] = type;
            if (!string.IsNullOrWhiteSpace(instance)) Data[nameof(Instance)] = instance;
            if (Errors.Count > 0) Data[nameof(Errors)] = Errors;
        }

        /// <summary>Exception mesajını tek satırda birleştirir.</summary>
        private static string BuildMessage(
            int status, string method, string uri, string? correlationId, string? title, string? detail)
        {
            var shortTitle = string.IsNullOrWhiteSpace(title) ? "" : $" | {title}";
            var shortDetail = string.IsNullOrWhiteSpace(detail) ? "" : $" | {detail}";
            var corr = string.IsNullOrWhiteSpace(correlationId) ? "" : $" | Correlation: {correlationId}";
            return $"HTTP {status} during {method} {uri}{shortTitle}{shortDetail}{corr}";
        }
    }
}
