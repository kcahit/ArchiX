using System;

namespace ArchiX.Library.Logging;

public sealed class LogTime
{
    public DateTimeOffset? ServerTimeUtc { get; set; }

    /// <summary>Server’ın kendi timezone’una göre zamanı (örn: Europe/Berlin).</summary>
    public DateTimeOffset? ServerLocalTime { get; set; }

    /// <summary>Config ile belirlenen timezone’a göre (örn: Europe/Istanbul).</summary>
    public DateTimeOffset? LocalTime { get; set; }

    public string? TimeZoneId { get; set; }
}
