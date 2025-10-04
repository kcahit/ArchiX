using System.Diagnostics;
using System.Diagnostics.Metrics;


namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// Uygulama genelinde hata metrikleri.
/// </summary>
public static class ErrorMetric
{
    /// <summary>
    /// Toplam hata sayısı sayacı.
    /// </summary>
    public static readonly Counter<long> ErrorsTotal =
        ArchiXTelemetry.Meter.CreateCounter<long>(
            name: "archix_errors_total",
            unit: "count",
            description: "Toplam hata sayısı");

    /// <summary>
    /// Hata oluştuğunda sayacı artırır.
    /// </summary>
    /// <param name="area">Hatanın bağlamı/katmanı.</param>
    /// <param name="exceptionName">İstisna tür adı (opsiyonel).</param>
    public static void Record(string? area = null, string? exceptionName = null)
    {
        var tags = new TagList();
        if (!string.IsNullOrEmpty(area)) tags.Add("area", area);
        if (!string.IsNullOrEmpty(exceptionName)) tags.Add("exception", exceptionName);

        ErrorsTotal.Add(1, tags);
    }
}
