using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ArchiX.Library.Runtime.Observability;

public sealed class ErrorMetric
{
    public const string MeterName = "ArchiX.Library";

    private readonly Counter<long> _total;

    public ErrorMetric(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);
        _total = meter.CreateCounter<long>(
            "archix_errors_total",
            unit: "count",
            description: "Toplam hata sayısı");
    }

    public void Record(string area, string exceptionName)
    {
        var tags = new TagList
        {
            { "area", string.IsNullOrWhiteSpace(area) ? "unknown" : area },
            { "exception", string.IsNullOrWhiteSpace(exceptionName) ? "unknown" : exceptionName }
        };

        _total.Add(1, tags);
    }
}
