using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiX.Library.Tests.Test.RunTimeTests.ObservabilityTests;

/// <summary>
/// Observability devre dışıyken /metrics’in yayınlanmadığını doğrular.
/// </summary>
public sealed class MetricsDisabledTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MetricsDisabledTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSolutionRelativeContentRoot("tests/ArchiX.Library.Tests");

            builder.UseSetting("DOTNET_ENVIRONMENT", "Testing");
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                var inmem = new Dictionary<string, string?>
                {
                    ["Observability:Enabled"] = "false"
                };
                cfg.AddInMemoryCollection(inmem!);
            });
        });
    }

    [Fact]
    public async Task Metrics_Endpoint_Should_Return_404_When_Disabled()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
