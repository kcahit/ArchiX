namespace ArchiX.Library.Logging;

/// <summary>
/// Log kaydının önem seviyesini ve hata bilgisini tutar.
/// </summary>
public sealed class LogSeverity
{
    /// <summary>
    /// Sayısal seviye değeri.
    /// Örn: Warning=1, Error=2, Critical=3
    /// </summary>
    public int? SeverityNumber { get; set; }

    /// <summary>
    /// Seviye adı.
    /// Örn: "Warning", "Error", "Critical"
    /// </summary>
    public string? SeverityName { get; set; }

    /// <summary>
    /// Hata kodu (ör. HResult veya ExceptionLogger code).
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Kullanıcıya uygun hata mesajı (ExceptionLogger mesajı).
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Ek hata detayları (sadece Development ortamında doldurulur).
    /// </summary>
    public string? Details { get; set; }
}
