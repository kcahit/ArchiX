namespace ArchiX.Library.Logging;

/// <summary>
/// Log kaydı için zaman bilgilerini içerir.
/// UTC, server local ve config ile belirlenen timezone değerlerini saklar.
/// </summary>
public sealed class LogTime
{
    /// <summary>
    /// Sunucu zamanı (UTC).
    /// Sabit referans olarak kullanılır.
    /// </summary>
    public DateTimeOffset? ServerTimeUtc { get; set; }

    /// <summary>
    /// Sunucunun kendi timezone’una göre zamanı.
    /// Örn: Europe/Berlin
    /// </summary>
    public DateTimeOffset? ServerLocalTime { get; set; }

    /// <summary>
    /// Konfigürasyonda belirtilen timezone’a göre zamanı.
    /// Örn: Europe/Istanbul
    /// </summary>
    public DateTimeOffset? LocalTime { get; set; }

    /// <summary>
    /// Kullanılan timezone ID’si.
    /// Fallback sonrası kesin değer yazılır.
    /// </summary>
    public string? TimeZoneId { get; set; }
}
