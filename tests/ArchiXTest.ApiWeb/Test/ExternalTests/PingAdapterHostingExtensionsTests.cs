// File: tests/ArchiXTest.ApiWeb/Test/ExternalTests/PingAdapterHostingExtensionsTests.cs
#nullable enable
using ArchiX.Library.External;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.ExternalTests
{
    /// <summary>AddPingAdapterWithHealthCheck uzantısı için kayıt testleri.</summary>
    public sealed class PingAdapterHostingExtensionsTests
    {
        [Fact]
        public void AddPingAdapterWithHealthCheck_RegistersAdapter_And_HealthCheck()
        {
            // Arrange
            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ExternalServices:DemoApi:BaseAddress"] = "http://localhost:5055/",
                    ["ExternalServices:DemoApi:TimeoutSeconds"] = "5"
                })
                .Build();

            var services = new ServiceCollection();

            // Act
            services.AddPingAdapterWithHealthCheck(cfg, "ExternalServices:DemoApi", "external_ping");
            var sp = services.BuildServiceProvider();

            // Assert: IPingAdapter çözümlenir
            var adapter = sp.GetService<IPingAdapter>();
            Assert.NotNull(adapter);

            // Assert: HealthCheck kaydı mevcut
            var hcOpts = sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            Assert.Contains(hcOpts.Registrations, r => string.Equals(r.Name, "external_ping", StringComparison.Ordinal));
        }
    }
}
