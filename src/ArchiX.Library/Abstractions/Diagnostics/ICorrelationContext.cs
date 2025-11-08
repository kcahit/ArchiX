namespace ArchiX.Library.Abstractions.Diagnostics;

/// <summary>
/// Ýstek korelasyon bilgisi sözleþmesi.
/// </summary>
public interface ICorrelationContext
{
 string CorrelationId { get; }
 string? TraceId { get; }
}
