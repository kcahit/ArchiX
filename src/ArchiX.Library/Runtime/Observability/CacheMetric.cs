using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// Cache işlemleri için metrikler.
/// </summary>
public static class CacheMetric
{
    /// <summary>
    /// Toplam cache işlem sayısı (get|set|remove) ve isabet durumu.
    /// </summary>
    public static readonly Counter<long> OpsTotal =
        ArchiXTelemetry.Meter.CreateCounter<long>(
            name: "archix_cache_ops_total",
            unit: "count",
            description: "Toplam cache işlem sayısı");

    /// <summary>
    /// Cache işlem süresi (ms).
    /// </summary>
    public static readonly Histogram<double> OpDurationMs =
        ArchiXTelemetry.Meter.CreateHistogram<double>(
            name: "archix_cache_op_duration_ms",
            unit: "ms",
            description: "Cache işlem süresi (ms)");

    /// <summary>
    /// Bir cache işlemini say ve süresini kaydet.
    /// </summary>
    /// <param name="op">İşlem türü: get|set|remove.</param>
    /// <param name="hit">Get için isabet durumu; diğer işlemler için false bırakılabilir.</param>
    /// <param name="elapsedMs">Geçen süre (ms).</param>
    public static void Record(string op, bool hit, double elapsedMs)
    {
        var tags = new TagList
        {
            { "op", op },
            { "hit", hit }
        };

        OpsTotal.Add(1, tags);
        OpDurationMs.Record(elapsedMs, tags);
    }
}
