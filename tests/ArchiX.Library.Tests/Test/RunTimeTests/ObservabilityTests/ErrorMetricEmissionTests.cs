using ArchiX.Library.Runtime.Observability;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiX.Library.Tests.Test.RunTimeTests.ObservabilityTests;

/// <summary>
/// ErrorMetric tetiklendikten sonra /metrics çıktısında yayımlandığını doğrular.
/// </summary>
public sealed class ErrorMetricEmissionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErrorMetricEmissionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSolutionRelativeContentRoot("tests/ArchiX.Library.Tests"); // içerik kökü düzeltildi
            builder.UseSetting("DOTNET_ENVIRONMENT", "Testing");
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                var inmem = new Dictionary<string, string?>
                {
                    ["Observability:Enabled"] = "true",
                    ["Observability:Metrics:Enabled"] = "true",
                    ["Observability:Metrics:Exporter"] = "prometheus",
                    ["Observability:Metrics:ScrapeEndpoint"] = "/metrics"
                };
                cfg.AddInMemoryCollection(inmem!);
            });
        });
    }

    /// <summary>
    /// ErrorMetric.Record çağrısı sonrası archix_errors_total metrik adını bekler.
    /// </summary>
    [Fact]
    public async Task ErrorMetric_Should_Appear_In_Prometheus_Scrape()
    {
        var client = _factory.CreateClient();

        ErrorMetric.Record(area: "test", exceptionName: "Manual");

        var metrics = await client.GetStringAsync("/metrics");
        Assert.Contains("archix_errors_total", metrics);
    }
}
