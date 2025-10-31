using ArchiX.Library.Runtime.Observability;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RunTimeTests.ObservabilityTests;

public sealed class ErrorMetricEmissionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErrorMetricEmissionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSolutionRelativeContentRoot("tests/ArchiX.Library.Tests");

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
    public async Task ErrorMetric_Should_Appear_In_Prometheus_Scrape()
    {
        var client = _factory.CreateClient();

        // 🔑 ErrorMetric artık statik değil, DI’dan resolve et
        var errorMetric = _factory.Services.GetRequiredService<ErrorMetric>();
        errorMetric.Record("test", "Manual");

        var metrics = await client.GetStringAsync("/metrics");
        Assert.Contains("archix_errors_total", metrics);
    }
}
