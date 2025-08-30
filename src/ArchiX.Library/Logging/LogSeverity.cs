namespace ArchiX.Library.Logging;

public sealed class LogSeverity
{
    public int? SeverityNumber { get; set; }   // Örn: Warning=1, Error=2, Critical=3
    public string? SeverityName { get; set; }  // "Warning", "Error", "Critical"
    public string? Code { get; set; }          // ExceptionLogger code (HResult vb.)
    public string? Message { get; set; }       // ExceptionLogger mesaj (kullanıcıya uygun)
    public string? Details { get; set; }       // Sadece Development ortamında
}
