using ArchiX.Library.Runtime.Observability;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RunTimeTests.ObservabilityTests;
public sealed class DbMetricEmissionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DbMetricEmissionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSolutionRelativeContentRoot("tests/ArchiX.Library.Tests"); // FIX

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

    [Fact]
    public async Task DbMetric_Should_Appear_In_Prometheus_Scrape()
    {
        var client = _factory.CreateClient();

        DbMetric.Record(op: "seed", elapsedMs: 12.3, success: true);

        var metrics = await client.GetStringAsync("/metrics");
        Assert.Contains("archix_db_ops_total", metrics);
        Assert.Contains("archix_db_op_duration_ms", metrics);
    }
}
