using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// ArchiX için ortak telemetri giriş noktaları. Activity ve Meter tanımlarını içerir.
/// </summary>
public static class ArchiXTelemetry
{
    /// <summary>
    /// Hizmet adı. Üretilen trace ve metric kaynak kimliği olarak kullanılır.
    /// </summary>
    public const string ServiceName = "ArchiX";

    /// <summary>
    /// Hizmet sürümü. Metric versiyonlaması ve kaynak tanımlamada kullanılır.
    /// </summary>
    public const string ServiceVersion = "1.0";

    /// <summary>
    /// Uygulama içi span/trace üretimi için Activity kaynağı.
    /// </summary>
    public static readonly ActivitySource Activity = new(ServiceName);

    /// <summary>
    /// Özel sayaç ve histogramlar için Metric kaynağı.
    /// </summary>
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);
}
