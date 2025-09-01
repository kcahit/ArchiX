using System;

namespace ArchiX.Library.Logging;

/// <summary>
/// Loglama konfigürasyon seçeneklerini temsil eder.
/// appsettings veya ENV üzerinden alınabilir.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>
    /// Log dosyalarının yazılacağı temel dizin.
    /// Varsayılan: C:\ArchiX\Logs\ArchiXTests\Api
    /// </summary>
    public string BasePath { get; set; } = @"C:\ArchiX\Logs\ArchiXTests\Api";

    /// <summary>
    /// Uygulama adı. Örn: ArchiXTests.Api
    /// </summary>
    public string AppName { get; set; } = "ArchiXTests.Api";

    /// <summary>
    /// Günlük dosya adı prefix’i.
    /// Varsayılan: errors
    /// </summary>
    public string DailyFilePrefix { get; set; } = "errors";

    /// <summary>
    /// Maksimum dosya boyutu (MB).
    /// Varsayılan: 50 MB (1–4096 MB arası).
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 50;

    /// <summary>
    /// Gün cinsinden saklama süresi.
    /// Varsayılan: 14 gün.
    /// </summary>
    public int RetainDays { get; set; } = 14;

    /// <summary>
    /// Yalnızca hata (error) kapsamındaki logları yaz.
    /// Varsayılan: true
    /// </summary>
    public bool ErrorOnly { get; set; } = true;

    /// <summary>
    /// Teslim modu: Db, Json veya her ikisi.
    /// Varsayılan: JsonOnly
    /// </summary>
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.JsonOnly;

    /// <summary>
    /// E-posta bildirimi aktif mi.
    /// Varsayılan: true (eşikler/alıcılar sonradan belirlenecek).
    /// </summary>
    public bool EmailEnabled { get; set; } = true;

    /// <summary>
    /// Zaman dilimi kimliği (örn: Europe/Istanbul).
    /// Varsayılan: Local timezone
    /// </summary>
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    /// <summary>
    /// MB → byte çevirisi (overflow korumalı).
    /// </summary>
    public long GetMaxFileSizeBytes()
    {
        var mb = Math.Clamp(MaxFileSizeMB, 1, 4096);
        return (long)mb * 1024L * 1024L;
    }
}
