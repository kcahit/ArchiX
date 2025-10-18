using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RunTimeTests.ObservabilityTests;

/// <summary>
/// Prometheus scraping yolunun konfigürasyonla değiştirilebildiğini doğrular.
/// </summary>
public sealed class MetricsCustomPathTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// ScrapeEndpoint'i /metricsz olarak ayarlar.
    /// </summary>
    /// <param name="factory">WebApplicationFactory.</param>
    public MetricsCustomPathTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSolutionRelativeContentRoot("tests/ArchiXTest.ApiWeb"); // FIX

            builder.UseSetting("DOTNET_ENVIRONMENT", "Testing");
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                var inmem = new Dictionary<string, string?>
                {
                    ["Observability:Enabled"] = "true",
                    ["Observability:Metrics:Enabled"] = "true",
                    ["Observability:Metrics:Exporter"] = "prometheus",
                    ["Observability:Metrics:ScrapeEndpoint"] = "/metricsz"
                };
                cfg.AddInMemoryCollection(inmem!);
            });
        });
    }

    /// <summary>
    /// Varsayılan /metrics 404 olur, yeni /metricsz 200 döner.
    /// </summary>
    [Fact]
    public async Task Custom_Scrape_Path_Should_Work()
    {
        var client = _factory.CreateClient();

        var respDefault = await client.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.NotFound, respDefault.StatusCode);

        var respCustom = await client.GetAsync("/metricsz");
        Assert.Equal(HttpStatusCode.OK, respCustom.StatusCode);
    }
}
