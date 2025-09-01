namespace ArchiX.Library.Logging;

/// <summary>
/// Tek bir log kaydını temsil eder. 
/// Zaman, seviye, korelasyon, HTTP, uygulama ve exception bilgilerini içerir.
/// </summary>
public sealed class LogRecord
{
    /// <summary>
    /// Zaman bilgileri (UTC, local, timezone).
    /// </summary>
    public LogTime? Time { get; set; }

    /// <summary>
    /// Log seviyesi ve hata kodu bilgisi.
    /// </summary>
    public LogSeverity? Severity { get; set; }

    /// <summary>
    /// Correlation ve Trace ID bilgileri.
    /// </summary>
    public LogCorrelation? Correlation { get; set; }

    /// <summary>
    /// HTTP isteği/yanıtı bilgileri.
    /// </summary>
    public LogHttp? Http { get; set; }

    /// <summary>
    /// Uygulama ve sunucu bilgileri.
    /// </summary>
    public LogApp? App { get; set; }

    /// <summary>
    /// Exception bilgileri.
    /// </summary>
    public LogException? Exception { get; set; }

    /// <summary>
    /// İşlemin toplam süresi (ms).
    /// Opsiyonel alan.
    /// </summary>
    public long? DurationMs { get; set; }
}
