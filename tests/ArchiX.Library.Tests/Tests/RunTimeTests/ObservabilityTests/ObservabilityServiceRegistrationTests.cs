using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RunTimeTests.ObservabilityTests;

public sealed class ObservabilityServiceRegistrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ObservabilityServiceRegistrationTests(WebApplicationFactory<Program> factory)
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
                    ["Observability:Tracing:Enabled"] = "true",
                    ["Observability:Metrics:Enabled"] = "true",
                    ["Observability:Metrics:Exporter"] = "prometheus",
                    ["Observability:Metrics:ScrapeEndpoint"] = "/metrics"
                };
                cfg.AddInMemoryCollection(inmem!);
            });
        });
    }

    // Mevcut test metod(lar)ınız aynen kalır.
}
