using ArchiX.Library.Diagnostics;

namespace ArchiX.Library.Logging;

/// <summary>
/// LogRecord nesnelerini üretmek için fabrika sınıfı.
/// </summary>
public static class LogRecordFactory
{
    /// <summary>
    /// Bir Exception nesnesinden LogRecord oluşturur.
    /// </summary>
    /// <param name="ex">Yakalanan exception.</param>
    /// <param name="status">HTTP status kodu.</param>
    /// <param name="method">HTTP method (GET, POST, ...).</param>
    /// <param name="path">Request path.</param>
    /// <param name="route">Route şablonu.</param>
    /// <param name="query">Query parametreleri.</param>
    /// <param name="headers">Header bilgileri.</param>
    /// <param name="clientIp">İstemci IP adresi.</param>
    /// <param name="userAgent">User-Agent bilgisi.</param>
    /// <param name="requestId">Request ID.</param>
    /// <param name="correlationId">Correlation ID.</param>
    /// <param name="traceId">Trace ID.</param>
    /// <param name="appName">Uygulama adı.</param>
    /// <param name="environment">Çalışma ortamı (Dev/Staging/Prod).</param>
    /// <param name="version">Uygulama versiyonu.</param>
    /// <param name="timeZoneId">Zaman dilimi ID (örn. Europe/Istanbul).</param>
    /// <param name="detailsIfDev">Sadece development modunda eklenen detaylar.</param>
    /// <param name="durationMs">İsteğin süresi (ms).</param>
    /// <returns>Hazır LogRecord nesnesi.</returns>
    public static LogRecord FromException(
        Exception ex,
        int status,
        string method,
        string path,
        string? route,
        IDictionary<string, string?>? query,
        IDictionary<string, string?>? headers,
        string? clientIp,
        string? userAgent,
        string? requestId,
        string correlationId,
        string? traceId,
        string appName,
        string environment,
        string version,
        string timeZoneId,
        string? detailsIfDev = null,
        long? durationMs = null)
    {
        // Exception türüne göre sade mesaj ve kodu ExceptionLogger’dan al
        var xlog = new ExceptionLogger(ex);

        // Zaman bilgileri
        var nowUtc = DateTimeOffset.UtcNow;
        var serverLocal = TimeZoneInfo.ConvertTime(nowUtc, TimeZoneInfo.Local);
        var tz = SafeFindTimeZone(timeZoneId);
        var local = TimeZoneInfo.ConvertTime(nowUtc, tz);

        return new LogRecord
        {
            Time = new LogTime
            {
                ServerTimeUtc = nowUtc,
                ServerLocalTime = serverLocal,
                LocalTime = local,
                TimeZoneId = tz.Id
            },
            Severity = new LogSeverity
            {
                SeverityNumber = 2, // Error varsayılan (middleware 400 için Warning’e çeviriyor)
                SeverityName = "Error",
                Code = xlog.hResult,
                Message = xlog.mesaj,
                Details = detailsIfDev
            },
            Correlation = new LogCorrelation
            {
                CorrelationId = correlationId,
                TraceId = traceId
            },
            Http = new LogHttp
            {
                Method = method,
                Path = path,
                Status = status,
                Route = route,
                Query = query is null ? null : new Dictionary<string, string?>(query),
                Headers = headers is null ? null : new Dictionary<string, string?>(headers),
                ClientIp = clientIp,
                UserAgent = userAgent,
                RequestId = requestId
            },
            App = new LogApp
            {
                App = appName,
                Version = version,
                Environment = environment,
                Machine = Environment.MachineName,
                ProcessId = Environment.ProcessId,
                ThreadId = Environment.CurrentManagedThreadId
            },
            Exception = new LogException
            {
                Type = ex.GetType().FullName,
                Message = ex.Message,
                HResult = ex.HResult,
                Source = ex.Source,
                TargetSite = ex.TargetSite?.Name,
                Stack = Truncate(ex.StackTrace, 16 * 1024),
                InnerCount = CountInner(ex)
            },
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Güvenli timezone bulucu (geçersiz ID gelirse Local’a düşer).
    /// </summary>
    private static TimeZoneInfo SafeFindTimeZone(string? id)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(id))
                return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch
        {
            // Geçersiz/uyumsuz id gelirse Local’a düş
        }
        return TimeZoneInfo.Local;
    }

    /// <summary>
    /// Inner exception sayısını döndürür.
    /// </summary>
    private static int CountInner(Exception ex)
    {
        int i = 0;
        var cur = ex.InnerException;
        while (cur != null)
        {
            i++;
            cur = cur.InnerException;
        }
        return i;
    }

    /// <summary>
    /// String’i maksimum uzunluğa kısaltır.
    /// </summary>
    private static string? Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max);
    }
}
