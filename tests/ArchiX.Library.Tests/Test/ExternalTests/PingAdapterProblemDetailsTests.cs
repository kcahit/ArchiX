// File: tests/ArchiXTest.ApiWeb/Test/ExternalTests/PingAdapterProblemDetailsTests.cs
#nullable enable
using System.Net;
using System.Text;

using ArchiX.Library.External;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.ExternalTests
{
    /// <summary>PingAdapter’ın RFC7807 ProblemDetails okuma davranışı.</summary>
    public sealed class PingAdapterProblemDetailsTests
    {
        [Fact]
        public async Task GetStatusTextAsync_ProblemJson_ThrowsWithProblemSummary()
        {
            // Arrange
            var problemJson = """
            {"type":"urn:archix:test","title":"Bad input","status":400,"detail":"name is required"}
            """;
            var handler = new ProblemHandler(HttpStatusCode.BadRequest, problemJson);
            using var http = new HttpClient(handler) { BaseAddress = new("http://example/") };
            var sut = new PingAdapter(http, NullLogger<PingAdapter>.Instance);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetStatusTextAsync());
            Assert.Contains("400", ex.Message);
            Assert.Contains("Bad input", ex.Message);
            Assert.Contains("name is required", ex.Message);
        }

        private sealed class ProblemHandler(HttpStatusCode code, string json) : HttpMessageHandler
        {
            private readonly HttpStatusCode _code = code;
            private readonly string _json = json;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var res = new HttpResponseMessage(_code)
                {
                    Content = new StringContent(_json, Encoding.UTF8, "application/problem+json")
                };
                return Task.FromResult(res);
            }
        }
    }
}
