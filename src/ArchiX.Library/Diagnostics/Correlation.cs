using System.Threading;

namespace ArchiX.Library.Diagnostics;

/// <summary>İstek zinciri korelasyon bilgisi.</summary>
public interface ICorrelationContext
{
    string CorrelationId { get; }
    string? TraceId { get; }
}

/// <summary>Basit korelasyon modeli.</summary>
public sealed class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; }
    public string? TraceId { get; }

    public CorrelationContext(string? correlationId = null, string? traceId = null)
    {
        CorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? Guid.NewGuid().ToString("n")
            : correlationId!;
        TraceId = traceId;
    }
}

/// <summary>Ambient (AsyncLocal) korelasyon tutucu.</summary>
public static class Correlation
{
    private static readonly AsyncLocal<ICorrelationContext?> _ambient = new();

    public static ICorrelationContext? Ambient => _ambient.Value;

    public static IDisposable BeginScope(string? correlationId = null, string? traceId = null)
    {
        var previous = _ambient.Value;
        _ambient.Value = new CorrelationContext(correlationId, traceId);
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly ICorrelationContext? _previous;
        public Scope(ICorrelationContext? previous) => _previous = previous;
        public void Dispose() => _ambient.Value = _previous;
    }
}
