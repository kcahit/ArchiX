namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// Gözlemlenebilirlik (tracing, metrics, logs) yapılandırma seçenekleri.
/// </summary>
public sealed class ObservabilityOptions
{
    /// <summary>Tüm gözlemlenebilirliği aç/kapat.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>İzleme (tracing) seçenekleri.</summary>
    public TracingOptions Tracing { get; set; } = new();

    /// <summary>Metrik seçenekleri.</summary>
    public MetricsOptions Metrics { get; set; } = new();

    /// <summary>Log seçenekleri.</summary>
    public LogsOptions Logs { get; set; } = new();

    /// <summary>Tracing için alt seçenekler.</summary>
    public sealed class TracingOptions
    {
        /// <summary>Tracing aç/kapat.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>İhracatçı türü. "otlp" veya "none".</summary>
        public string Exporter { get; set; } = "otlp";

        /// <summary>OTLP endpoint (ör. http://localhost:4317).</summary>
        public string? OtlpEndpoint { get; set; }
    }

    /// <summary>Metrikler için alt seçenekler.</summary>
    public sealed class MetricsOptions
    {
        /// <summary>Metrics aç/kapat.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>İhracatçı türü. "prometheus", "otlp" veya "none".</summary>
        public string Exporter { get; set; } = "prometheus";

        /// <summary>Prometheus scrape yolu (varsayılan: /metrics).</summary>
        public string ScrapeEndpoint { get; set; } = "/metrics";

        /// <summary>OTLP endpoint (opsiyonel).</summary>
        public string? OtlpEndpoint { get; set; }
    }

    /// <summary>Loglar için alt seçenekler.</summary>
    public sealed class LogsOptions
    {
        /// <summary>Loglar aç/kapat.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>İhracatçı türü. "otlp" veya "none".</summary>
        public string Exporter { get; set; } = "otlp";

        /// <summary>OTLP endpoint (opsiyonel).</summary>
        public string? OtlpEndpoint { get; set; }
    }
}
