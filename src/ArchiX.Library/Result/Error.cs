using System;

namespace ArchiX.Library.Result
{
    /// <summary>
    /// Hata bilgisini temsil eden model sınıfıdır.
    /// Basit (code+message) veya detaylı (log bilgileri) kullanım için uygundur.
    /// </summary>
    public class Error
    {
        /// <summary> Hata kodu (ör: E1001). </summary>
        public string Code { get; }

        /// <summary> Hata mesajı. </summary>
        public string Message { get; }

        /// <summary> İstekle ilişkili CorrelationId. </summary>
        public string? CorrelationId { get; set; }

        /// <summary> Hatanın oluştuğu zaman. </summary>
        public DateTimeOffset? Time { get; set; }

        /// <summary> TraceId bilgisi. </summary>
        public string? TraceId { get; set; }

        /// <summary> Hata detay mesajı. </summary>
        public string? Details { get; set; }

        /// <summary> Hata kaynağı (Exception.Source). </summary>
        public string? Source { get; set; }

        /// <summary> Hatanın gerçekleştiği metot (Exception.TargetSite). </summary>
        public string? TargetSite { get; set; }

        /// <summary> Hata şiddeti (Info, Warning, Error, Critical). </summary>
        public ErrorSeverity? Severity { get; set; } = ErrorSeverity.None;

        /// <summary> Stack trace bilgisi. </summary>
        public string? Stack { get; set; }

        /// <summary>
        /// Hata olmadığı durumda kullanılacak özel nesne.
        /// "Hiç hata yok" anlamına gelir.
        /// </summary>
        public static Error None => new Error("NONE", string.Empty);

        /// <summary>
        /// Basit kurucu: sadece kod ve mesaj.
        /// </summary>
        /// <param name="code">Hata kodu.</param>
        /// <param name="message">Hata mesajı.</param>
        public Error(string code, string message)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Detaylı kurucu: hata hakkında tüm alanları set edebilmek için.
        /// </summary>
        /// <param name="code">Hata kodu.</param>
        /// <param name="message">Hata mesajı.</param>
        /// <param name="correlationId">CorrelationId bilgisi.</param>
        /// <param name="time">Hatanın oluştuğu zaman.</param>
        /// <param name="traceId">TraceId bilgisi.</param>
        /// <param name="details">Hata detay mesajı.</param>
        /// <param name="source">Hata kaynağı.</param>
        /// <param name="targetSite">Hatanın gerçekleştiği metot.</param>
        /// <param name="severity">Hata şiddeti (nullable).</param>
        /// <param name="stack">Stack trace bilgisi (opsiyonel).</param>
        public Error(
            string code,
            string message,
            string? correlationId,
            DateTimeOffset? time,
            string? traceId,
            string? details,
            string? source,
            string? targetSite,
            ErrorSeverity? severity,
            string? stack = null)
        {
            Code = code;
            Message = message;
            CorrelationId = correlationId;
            Time = time;
            TraceId = traceId;
            Details = details;
            Source = source;
            TargetSite = targetSite;
            Severity = severity;
            Stack = stack;
        }
    }
}
