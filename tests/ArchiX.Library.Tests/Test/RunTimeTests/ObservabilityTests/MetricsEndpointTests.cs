using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiX.Library.Tests.Test.RunTimeTests.ObservabilityTests;

/// <summary>
/// Prometheus scraping endpoint’in ayağa kalktığını doğrular.
/// </summary>
public sealed class MetricsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MetricsEndpointTests(WebApplicationFactory<Program> factory)
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
    public async Task Metrics_Endpoint_Should_Return_200_With_Content()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var text = await resp.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(text));
    }
}
