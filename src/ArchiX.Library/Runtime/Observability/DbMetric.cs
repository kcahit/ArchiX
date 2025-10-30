// File: src/ArchiX.Library/Runtime/Observability/DbMetric.cs
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>DB işlemleri için özel metrikler.</summary>
public static class DbMetric
{
    /// <summary>Toplam DB işlem sayısı.</summary>
    public static readonly Counter<long> OpsTotal =
        ArchiXTelemetry.Meter.CreateCounter<long>(
            name: "archix_db_ops_total",
            unit: "count",
            description: "Toplam DB işlem sayısı");

    /// <summary>DB işlem süreleri (ms).</summary>
    public static readonly Histogram<double> DurationMs =
        ArchiXTelemetry.Meter.CreateHistogram<double>(
            name: "archix_db_op_duration_ms",
            unit: "ms",
            description: "DB işlem süresi (ms)");

    /// <summary>Bir DB işlemini say ve süresini kaydet.</summary>
    public static void Record(string op, double elapsedMs, bool success)
    {
        var tags = new TagList { { "op", op }, { "success", success } };
        OpsTotal.Add(1, tags);
        DurationMs.Record(elapsedMs, tags);
    }
}
