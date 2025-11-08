// File: tests/ArchiX.Library.Tests.Tests.ExternalTests/PingControllerTests.cs
#nullable enable
using ArchiX.Library.Tests.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArchiX.Library.Tests.Tests.ExternalTests
{
    public sealed class PingControllerTests
    {
        private sealed class FakeAdapter : ArchiX.Library.Abstractions.External.IPingAdapter
        {
            public Task<string> GetStatusTextAsync(CancellationToken ct = default)
                => Task.FromResult("""{"service":"demo","version":"1.0","uptime":"123s"}""");
        }

        [Fact]
        public async Task GetStatus_Returns_TextPlain_Utf8()
        {
            var controller = new PingController(new FakeAdapter(), NullLogger<PingController>.Instance);

            var result = await controller.GetStatus(default);
            var cr = Assert.IsType<ContentResult>(result);

            Assert.Equal("pong", cr.Content);
            Assert.Equal("text/plain; charset=utf-8", cr.ContentType);
            Assert.Equal(200, cr.StatusCode);
        }

        [Fact]
        public async Task GetStatusJson_Returns_Json_Utf8()
        {
            var controller = new PingController(new FakeAdapter(), NullLogger<PingController>.Instance);

            var result = await controller.GetStatusJson(default);
            var cr = Assert.IsType<ContentResult>(result);

            Assert.Equal("application/json; charset=utf-8", cr.ContentType);
            Assert.Equal(200, cr.StatusCode);
            Assert.Contains(@"""service"":""demo""", cr.Content);
        }
    }
}
