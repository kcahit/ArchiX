using ArchiX.Library.Diagnostics;
using ArchiX.Library.Time;

namespace ArchiX.Library.Result;

/// <summary>
/// Hibrit Osman Çekirdeği hata modeli:
/// - Code / Message (kullanıcıya uygun mesaj)
/// - Details (geliştirici için ayrıntı; prod'da gizlenebilir)
/// - ServerTimeUtc (her zaman UTC)
/// - LocalTime + TimeZoneId (istemci bildirirse set edilebilir)
/// - CorrelationId / TraceId (log korelasyonu)
/// - Severity
/// </summary>
public readonly record struct Error(
    string Code,
    string Message,
    string? Details,
    DateTimeOffset ServerTimeUtc,
    DateTimeOffset? LocalTime,
    string? TimeZoneId,
    string? CorrelationId,
    string? TraceId,
    ErrorSeverity Severity)
{
    public static readonly Error None =
        new("", "", null, default, null, null, null, null, ErrorSeverity.None);

    /// <summary>Kolay oluşturucu (clock ve korelasyon otomatik).</summary>
    public static Error Create(
        string code,
        string message,
        string? details = null,
        IClock? clock = null,
        DateTimeOffset? localTime = null,
        string? timeZoneId = null,
        string? correlationId = null,
        string? traceId = null,
        ErrorSeverity severity = ErrorSeverity.Error)
    {
        var now = (clock ?? new SystemClock()).UtcNow;
        correlationId ??= Correlation.Ambient?.CorrelationId;
        traceId ??= Correlation.Ambient?.TraceId;

        return new Error(
            Code: code,
            Message: message,
            Details: details,
            ServerTimeUtc: now,
            LocalTime: localTime,
            TimeZoneId: timeZoneId,
            CorrelationId: correlationId,
            TraceId: traceId,
            Severity: severity
        );
    }
}
