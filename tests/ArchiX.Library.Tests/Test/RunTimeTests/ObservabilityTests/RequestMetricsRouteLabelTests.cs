// File: tests/ArchiXTest.ApiWeb/Test/RunTimeTests/ObservabilityTests/RequestMetricsRouteLabelTests.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RunTimeTests.ObservabilityTests;

/// <summary>Metrics middleware’in route bilgisini etiketlere eklediğini doğrular.</summary>
public sealed class RequestMetricsRouteLabelTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RequestMetricsRouteLabelTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSolutionRelativeContentRoot("tests/ArchiXTest.ApiWeb");
            builder.UseSetting("DOTNET_ENVIRONMENT", "Testing");

            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:Enabled"] = "true",
                    ["Observability:Metrics:Enabled"] = "true",
                    ["Observability:Metrics:Exporter"] = "prometheus",
                    ["Observability:Metrics:ScrapeEndpoint"] = "/metrics"
                }!);
            });

            // Test controller’ı ekle
            builder.ConfigureTestServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(ProbeController).Assembly);
            });
        });
    }

    [Fact]
    public async Task Metrics_Should_Contain_Route_Label()
    {
        var client = _factory.CreateClient();

        // DI bağımsız basit endpoint
        _ = await client.GetAsync("/__probe/status");

        var metrics = await client.GetStringAsync("/metrics");
        var lines = metrics.Split('\n').Where(l => l.Length > 0 && l[0] != '#');

        var hasRoute = lines.Any(l =>
            (l.Contains("archix_http_requests_total") || l.Contains("archix_http_request_duration_ms")) &&
            (l.Contains("route=\"/__probe/status\"") || l.Contains("/__probe/status")));

        Assert.True(hasRoute);
    }

    // Test-only controller
    [ApiController]
    [Route("/__probe")]
    public sealed class ProbeController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult Status() => Content("ok", "text/plain");
    }
}
