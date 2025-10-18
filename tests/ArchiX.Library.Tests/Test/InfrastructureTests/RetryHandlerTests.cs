// File: tests/ArchiXTest.ApiWeb/Tests/InfrastructureTests/RetryHandlerTests.cs
using System.Net;
using System.Text;

using ArchiX.Library.Infrastructure.Http;

using Xunit;

namespace ArchiXTest.ApiWeb.Tests.InfrastructureTests.Http
{
    public class RetryHandlerTests
    {
        [Fact]
        public async Task PostBody_Is_Preserved_On_Retry_And_ContentType_Remains()
        {
            // Arrange
            var json = "{\"a\":1}";
            byte[]? firstBytes = null, secondBytes = null;
            string? firstCt = null, secondCt = null;

            var retry = new RetryHandler(maxRetries: 1, baseDelay: TimeSpan.Zero);
            var fake = new LambdaHandler(async (req, attempt) =>
            {
                var bytes = req.Content is null ? Array.Empty<byte>() : await req.Content.ReadAsByteArrayAsync();
                var ct = req.Content?.Headers?.ContentType?.MediaType;

                if (attempt == 0)
                {
                    firstBytes = bytes;
                    firstCt = ct;
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError); // 500 -> retry
                }

                secondBytes = bytes;
                secondCt = ct;
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") };
            });

            retry.InnerHandler = fake;

            using var request = new HttpRequestMessage(HttpMethod.Post, "http://test/echo")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Act
            using var invoker = new HttpMessageInvoker(retry);
            using var response = await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(firstBytes);
            Assert.NotNull(secondBytes);
            Assert.Equal(firstBytes, secondBytes);                   // gövde aynı
            Assert.Equal("application/json", firstCt);               // content-type korunur
            Assert.Equal("application/json", secondCt);
            Assert.Equal(2, fake.Attempts);                          // 1 hata + 1 başarı
        }

        [Fact]
        public async Task Retries_On_5xx_Until_Success()
        {
            // Arrange
            var retry = new RetryHandler(maxRetries: 2, baseDelay: TimeSpan.Zero);
            var fake = new LambdaHandler((_, attempt) =>
            {
                // 0: 500, 1: 500, 2: 200
                return Task.FromResult(attempt < 2
                    ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    : new HttpResponseMessage(HttpStatusCode.OK));
            });
            retry.InnerHandler = fake;

            using var req = new HttpRequestMessage(HttpMethod.Get, "http://test/unstable");
            using var invoker = new HttpMessageInvoker(retry);

            // Act
            using var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Equal(3, fake.Attempts); // 2 retry + 1 başarı
        }

        [Fact]
        public async Task Does_Not_Retry_On_404()
        {
            // Arrange
            var retry = new RetryHandler(maxRetries: 5, baseDelay: TimeSpan.Zero);
            var fake = new LambdaHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound))); // 404 -> no retry
            retry.InnerHandler = fake;

            using var req = new HttpRequestMessage(HttpMethod.Get, "http://test/missing");
            using var invoker = new HttpMessageInvoker(retry);

            // Act
            using var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
            Assert.Equal(1, fake.Attempts); // tek deneme
        }

        [Fact]
        public async Task Retries_On_429()
        {
            // Arrange
            var retry = new RetryHandler(maxRetries: 1, baseDelay: TimeSpan.Zero);
            var fake = new LambdaHandler((_, attempt) =>
            {
                if (attempt == 0)
                {
                    var tooMany = new HttpResponseMessage((HttpStatusCode)429);
                    // Retry-After olmasa da RetryHandler 429’u retry eder
                    return Task.FromResult(tooMany);
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });
            retry.InnerHandler = fake;

            using var req = new HttpRequestMessage(HttpMethod.Get, "http://test/rate");
            using var invoker = new HttpMessageInvoker(retry);

            // Act
            using var resp = await invoker.SendAsync(req, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Equal(2, fake.Attempts);
        }

        // ---- yardımcı fake handler ----
        private sealed class LambdaHandler : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, int, Task<HttpResponseMessage>> _impl;
            public int Attempts { get; private set; }

            public LambdaHandler(Func<HttpRequestMessage, int, Task<HttpResponseMessage>> impl)
            {
                _impl = impl ?? throw new ArgumentNullException(nameof(impl));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var attempt = Attempts++;
                return _impl(request, attempt);
            }
        }
    }
}
