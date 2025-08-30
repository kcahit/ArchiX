using ArchiX.Library.Diagnostics;

namespace ArchiX.Library.Logging;

public static class LogRecordFactory
{
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
                ServerTimeUtc = nowUtc,          // Sabit referans (UTC)
                ServerLocalTime = serverLocal,   // Server’ın kendi lokal saati (örn. Europe/Berlin)
                LocalTime = local,               // Config ile belirlenen TZ (örn. Europe/Istanbul)
                TimeZoneId = tz.Id               // Kullanılan TZ kimliği (fallback sonrası kesin)
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

    private static string? Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max);
    }
}
