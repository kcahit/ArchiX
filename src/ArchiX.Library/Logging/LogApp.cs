namespace ArchiX.Library.Logging;

public sealed class LogApp
{
    public string? App { get; set; }           // Örn: ArchiXTests.Api
    public string? Version { get; set; }       // Örn: 1.0.0+build
    public string? Environment { get; set; }   // Development/Staging/Production
    public string? Machine { get; set; }       // Hostname
    public int? ProcessId { get; set; }
    public int? ThreadId { get; set; }
}
