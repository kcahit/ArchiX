using System;

namespace ArchiX.Library.Logging;

public sealed class LoggingOptions
{
    /// <summary>Varsayılan: C:\ArchiX\Logs\ArchiXTests\Api</summary>
    public string BasePath { get; set; } = @"C:\ArchiX\Logs\ArchiXTests\Api";

    /// <summary>Uygulama adı. Örn: ArchiXTests.Api</summary>
    public string AppName { get; set; } = "ArchiXTests.Api";

    /// <summary>Günlük dosya adı prefix’i. Varsayılan: errors</summary>
    public string DailyFilePrefix { get; set; } = "errors";

    /// <summary>
    /// Maksimum dosya boyutu (MB). Sistem bazında parametre.
    /// appsettings veya ENV ile verilecektir. Varsayılan: 50 MB.
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 50;

    /// <summary>Gün cinsinden saklama süresi. Varsayılan: 14 gün.</summary>
    public int RetainDays { get; set; } = 14;

    /// <summary>Sadece hata (error) kapsamı yazacağız.</summary>
    public bool ErrorOnly { get; set; } = true;

    /// <summary>Teslim modu: Db, JSON, ya da her ikisi.</summary>
    public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.JsonOnly;

    /// <summary>E-posta bildirimi mimaride açık; eşikler/alıcılar/config sonradan belirlenecek.</summary>
    public bool EmailEnabled { get; set; } = true;

    /// <summary>Zaman dilimi kimliği (örn: Europe/Istanbul)</summary>
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    /// <summary>İç kullanım: MB→byte çevirisi (overflow korumalı).</summary>
    public long GetMaxFileSizeBytes()
    {
        var mb = Math.Clamp(MaxFileSizeMB, 1, 4096);
        return (long)mb * 1024L * 1024L;
    }
}
