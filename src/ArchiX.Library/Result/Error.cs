namespace ArchiX.Library.Result
{
    /// <summary>
    /// Uygulama genelinde hata modelini temsil eder.
    /// </summary>
    public sealed class Error : IEquatable<Error>
    {
        /// <summary>Hata kodunu temsil eder (örnek: VAL001).</summary>
        public string Code { get; }

        /// <summary>Hata mesajını içerir.</summary>
        public string Message { get; }

        /// <summary>Ek hata detaylarını içerir (opsiyonel).</summary>
        public string? Details { get; }

        /// <summary>Sunucunun UTC zaman damgası.</summary>
        public DateTimeOffset? ServerTimeUtc { get; }

        /// <summary>Yerel zaman damgası (istemciye göre).</summary>
        public DateTimeOffset? LocalTime { get; }

        /// <summary>Zaman dilimi kimliği (örnek: Europe/Istanbul).</summary>
        public string? TimeZoneId { get; }

        /// <summary>İstek ile ilişkili korelasyon kimliği.</summary>
        public string? CorrelationId { get; set; }

        /// <summary>İstek için izleme kimliği (trace id).</summary>
        public string? TraceId { get; set; }

        /// <summary>Hata ciddiyet seviyesini belirtir.</summary>
        public ErrorSeverity Severity { get; }

        /// <summary>Hiçbir hata olmadığını temsil eden sabit değer.</summary>
        public static readonly Error None = new("NONE", string.Empty);

        /// <summary>
        /// Yeni bir hata nesnesi oluşturur.
        /// </summary>
        public Error(
            string code,
            string message,
            string? details = null,
            DateTimeOffset? serverTimeUtc = null,
            DateTimeOffset? localTime = null,
            string? timeZoneId = null,
            string? correlationId = null,
            string? traceId = null,
            ErrorSeverity severity = ErrorSeverity.None)
        {
            Code = code;
            Message = message;
            Details = details;
            ServerTimeUtc = serverTimeUtc;
            LocalTime = localTime;
            TimeZoneId = timeZoneId;
            CorrelationId = correlationId;
            TraceId = traceId;
            Severity = severity;
        }

        /// <summary>
        /// Nesneler arası eşitlik kontrolü (referans yerine değer bazlı).
        /// </summary>
        public bool Equals(Error? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Code == other.Code &&
                   Message == other.Message &&
                   Details == other.Details &&
                   ServerTimeUtc == other.ServerTimeUtc &&
                   LocalTime == other.LocalTime &&
                   TimeZoneId == other.TimeZoneId &&
                   CorrelationId == other.CorrelationId &&
                   TraceId == other.TraceId &&
                   Severity == other.Severity;
        }

        /// <summary>
        /// Obje tabanlı eşitlik kontrolü.
        /// </summary>
        public override bool Equals(object? obj) => Equals(obj as Error);

        /// <summary>
        /// Nesne için hash kodu üretir.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Code);
            hash.Add(Message);
            hash.Add(Details);
            hash.Add(ServerTimeUtc);
            hash.Add(LocalTime);
            hash.Add(TimeZoneId);
            hash.Add(CorrelationId);
            hash.Add(TraceId);
            hash.Add(Severity);
            return hash.ToHashCode();
        }

        /// <summary>
        /// İki Error nesnesi eşitse true döner.
        /// </summary>
        public static bool operator ==(Error? left, Error? right) =>
            Equals(left, right);

        /// <summary>
        /// İki Error nesnesi eşit değilse true döner.
        /// </summary>
        public static bool operator !=(Error? left, Error? right) =>
            !Equals(left, right);
    }
}
