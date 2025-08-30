namespace ArchiX.Library.Logging;

public sealed class LogException
{
    public string? Type { get; set; }
    public string? Message { get; set; }
    public int? HResult { get; set; }
    public string? Source { get; set; }
    public string? TargetSite { get; set; }
    public string? Stack { get; set; }         // Truncated versiyon
    public int? InnerCount { get; set; }
}
