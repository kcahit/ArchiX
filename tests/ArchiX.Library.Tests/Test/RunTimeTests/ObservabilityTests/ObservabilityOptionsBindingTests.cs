using ArchiX.Library.Runtime.Observability;

using Xunit;

namespace ArchiX.Library.Tests.Test.RunTimeTests.ObservabilityTests;

/// <summary>
/// ObservabilityOptions yapılandırma bağlamayı doğrular.
/// </summary>
public sealed class ObservabilityOptionsBindingTests
{
    [Fact]
    public void Should_Bind_All_Sections()
    {
        var inmem = new Dictionary<string, string?>
        {
            ["Observability:Enabled"] = "true",
            ["Observability:Tracing:Enabled"] = "true",
            ["Observability:Tracing:Exporter"] = "otlp",
            ["Observability:Tracing:OtlpEndpoint"] = "http://collector:4317",
            ["Observability:Metrics:Enabled"] = "true",
            ["Observability:Metrics:Exporter"] = "prometheus",
            ["Observability:Metrics:ScrapeEndpoint"] = "/metricsz",
            ["Observability:Logs:Enabled"] = "false",
            ["Observability:Logs:Exporter"] = "none"
        };

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(inmem!)
            .Build();

        var opt = new ObservabilityOptions();
        cfg.GetSection("Observability").Bind(opt);

        Assert.True(opt.Enabled);
        Assert.True(opt.Tracing.Enabled);
        Assert.Equal("otlp", opt.Tracing.Exporter);
        Assert.Equal("http://collector:4317", opt.Tracing.OtlpEndpoint);

        Assert.True(opt.Metrics.Enabled);
        Assert.Equal("prometheus", opt.Metrics.Exporter);
        Assert.Equal("/metricsz", opt.Metrics.ScrapeEndpoint);

        Assert.False(opt.Logs.Enabled);
        Assert.Equal("none", opt.Logs.Exporter);
    }
}
