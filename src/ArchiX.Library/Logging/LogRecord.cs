namespace ArchiX.Library.Logging;

public sealed class LogRecord
{
    // 1) Time
    public LogTime? Time { get; set; }

    // 2) Severity & Code
    public LogSeverity? Severity { get; set; }

    // 3) Correlation
    public LogCorrelation? Correlation { get; set; }

    // 4) HTTP
    public LogHttp? Http { get; set; }

    // 5) App/Server
    public LogApp? App { get; set; }

    // 6) Exception
    public LogException? Exception { get; set; }

    // 7) Opsiyonel
    public long? DurationMs { get; set; }
}
