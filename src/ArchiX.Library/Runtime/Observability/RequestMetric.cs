using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// HTTP istek metrikleri.
/// </summary>
public static class RequestMetric
{
    /// <summary>
    /// Toplam istek sayısı.
    /// </summary>
    public static readonly Counter<long> RequestsTotal =
        ArchiXTelemetry.Meter.CreateCounter<long>(
            name: "archix_http_requests_total",
            unit: "count",
            description: "Toplam HTTP istek sayısı");

    /// <summary>
    /// İstek süreleri (ms).
    /// </summary>
    public static readonly Histogram<double> DurationMs =
        ArchiXTelemetry.Meter.CreateHistogram<double>(
            name: "archix_http_request_duration_ms",
            unit: "ms",
            description: "HTTP istek süresi (ms)");

    /// <summary>
    /// Bir isteği say ve süresini kaydet.
    /// </summary>
    /// <param name="method">HTTP metodu.</param>
    /// <param name="route">Mantıksal rota (opsiyonel).</param>
    /// <param name="statusCode">HTTP durum kodu.</param>
    /// <param name="elapsedMs">Geçen süre (ms).</param>
    public static void Record(string method, string? route, int statusCode, double elapsedMs)
    {
        var tags = new TagList
        {
            { "method", method },
            { "status", statusCode },
            { "success", statusCode is >=200 and <400 }
        };
        if (!string.IsNullOrEmpty(route)) tags.Add("route", route);

        RequestsTotal.Add(1, tags);
        DurationMs.Record(elapsedMs, tags);
    }
}
