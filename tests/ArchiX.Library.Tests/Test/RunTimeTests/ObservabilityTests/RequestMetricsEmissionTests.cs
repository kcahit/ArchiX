using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiX.Library.Tests.Test.RunTimeTests.ObservabilityTests;

public sealed class RequestMetricsEmissionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RequestMetricsEmissionTests(WebApplicationFactory<Program> factory)
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
    public async Task Metrics_Should_Contain_Http_Request_Metrics()
    {
        var client = _factory.CreateClient();
        _ = await client.GetAsync("/healthz");

        var metrics = await client.GetStringAsync("/metrics");
        Assert.Contains("archix_http_request_duration_ms", metrics);
        Assert.Contains("archix_http_requests_total", metrics);
    }
}
