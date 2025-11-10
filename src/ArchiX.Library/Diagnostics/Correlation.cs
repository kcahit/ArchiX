namespace ArchiX.Library.Diagnostics
{
 /// <summary>
 /// Basit korelasyon modeli implementasyonu; Abstractions arayüzünü uygular.
 /// </summary>
 public sealed class CorrelationContext : ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext
 {
 /// <summary>Korelasyon kimliði.</summary>
 public string CorrelationId { get; }

 /// <summary>Ýzleme (Trace) kimliði.</summary>
 public string? TraceId { get; }

 /// <summary>
 /// Yeni bir <see cref="CorrelationContext"/> oluþturur.
 /// <paramref name="correlationId"/> boþ ise yeni GUID (n) atanýr.
 /// </summary>
 public CorrelationContext(string? correlationId = null, string? traceId = null)
 {
 CorrelationId = string.IsNullOrWhiteSpace(correlationId)
 ? System.Guid.NewGuid().ToString("n")
 : correlationId!;
 TraceId = traceId;
 }
 }

 /// <summary>
 /// Ambient (AsyncLocal) tabanlý korelasyon tutucu.
 /// </summary>
 public static class Correlation
 {
 private static readonly System.Threading.AsyncLocal<ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext?> _ambient = new();

 /// <summary>Geçerli korelasyon baðlamý (ambient).</summary>
 public static ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext? Ambient => _ambient.Value;

 /// <summary>Yeni bir korelasyon kapsamý baþlatýr.</summary>
 public static System.IDisposable BeginScope(string? correlationId = null, string? traceId = null)
 {
 var previous = _ambient.Value;
 _ambient.Value = new CorrelationContext(correlationId, traceId);
 return new Scope(previous);
 }

 /// <summary>Scope yaþamýný yöneten iç sýnýf.</summary>
 private sealed class Scope : System.IDisposable
 {
 private readonly ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext? _previous;
 public Scope(ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext? previous) => _previous = previous;
 public void Dispose() => _ambient.Value = _previous;
 }
 }
}
