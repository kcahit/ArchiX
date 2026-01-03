using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ArchiX.Library.Runtime.Observability;

public sealed class DbMetric
{
    public const string MeterName = "ArchiX.Library";

    private readonly Counter<long> _total;
    private readonly Histogram<double> _duration;

    public DbMetric(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);

        _total = meter.CreateCounter<long>(
            "archix_db_ops_total",
            unit: "count",
            description: "Toplam veritabanı işlemleri");

        _duration = meter.CreateHistogram<double>(
            "archix_db_op_duration_ms",
            unit: "ms",
            description: "Veritabanı işlemlerinin süresi");
    }

    public void Record(string op, double elapsedMs, bool success)
    {
        var tags = new TagList
        {
            { "op", string.IsNullOrWhiteSpace(op) ? "unknown" : op },
            { "success", success }
        };

        _total.Add(1, tags);
        _duration.Record(elapsedMs, tags);
    }
}
