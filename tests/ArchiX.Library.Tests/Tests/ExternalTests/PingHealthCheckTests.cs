// File: tests/ ArchiX.Library.Tests.Tests.ExternalTests/PingHealthCheckTests.cs
#nullable enable
using ArchiX.Library.External;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using Xunit;

namespace ArchiX.Library.Tests.Tests.ExternalTests
{
    public sealed class PingHealthCheckTests
    {
        [Fact]
        public async Task CheckHealthAsync_WhenAdapterOk_ReturnsHealthy()
        {
            // Arrange
            IPingAdapter fake = new FakePingAdapterOk("pong");
            var hc = new PingHealthCheck(fake);

            // Act
            var result = await hc.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenAdapterFails_ReturnsUnhealthy()
        {
            // Arrange
            IPingAdapter fake = new FakePingAdapterFail(new InvalidOperationException("down"));
            var hc = new PingHealthCheck(fake);

            // Act
            var result = await hc.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.NotNull(result.Exception);
        }

        private sealed class FakePingAdapterOk(string text) : IPingAdapter
        {
            private readonly string _text = text;
            public Task<string> GetStatusTextAsync(CancellationToken ct = default) => Task.FromResult(_text);
        }

        private sealed class FakePingAdapterFail(Exception ex) : IPingAdapter
        {
            private readonly Exception _ex = ex;
            public Task<string> GetStatusTextAsync(CancellationToken ct = default) => Task.FromException<string>(_ex);
        }
    }
}
