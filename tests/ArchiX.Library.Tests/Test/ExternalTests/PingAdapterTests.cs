// File: tests/ArchiXTest.ApiWeb/Test/ExternalTests/PingAdapterTests.cs
#nullable enable
using System.Net;
using System.Text;

using ArchiX.Library.External;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.ExternalTests
{
    /// <summary>PingAdapter için temel davranış testleri.</summary>
    public sealed class PingAdapterTests
    {
        /// <summary>Başarılı yanıtta gövde metni döner.</summary>
        [Fact]
        public async Task GetStatusTextAsync_Success_ReturnsBody()
        {
            // Arrange
            var handler = new FakeHandler(HttpStatusCode.OK, "pong");
            using var http = new HttpClient(handler) { BaseAddress = new("http://example/") };
            var sut = new PingAdapter(http, NullLogger<PingAdapter>.Instance);

            // Act
            var text = await sut.GetStatusTextAsync();

            // Assert
            Assert.Equal("pong", text);
        }

        /// <summary>Başarısız yanıtta HttpRequestException fırlatılır ve gövde mesajda yer alır.</summary>
        [Fact]
        public async Task GetStatusTextAsync_Failure_ThrowsWithBody()
        {
            // Arrange
            var handler = new FakeHandler(HttpStatusCode.ServiceUnavailable, "down");
            using var http = new HttpClient(handler) { BaseAddress = new("http://example/") };
            var sut = new PingAdapter(http, NullLogger<PingAdapter>.Instance);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetStatusTextAsync());
            Assert.Contains("503", ex.Message);
            Assert.Contains("down", ex.Message);
        }

        /// <summary>HttpClient için sahte handler.</summary>
        private sealed class FakeHandler(HttpStatusCode code, string body) : HttpMessageHandler
        {
            private readonly HttpStatusCode _code = code;
            private readonly string _body = body;

            /// <summary>İstekleri sabit yanıtla döndürür.</summary>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var res = new HttpResponseMessage(_code)
                {
                    Content = new StringContent(_body, Encoding.UTF8, "text/plain")
                };
                return Task.FromResult(res);
            }
        }
    }
}
