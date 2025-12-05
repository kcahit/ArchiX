using System.Net;

using ArchiX.Library.Runtime.Security;

using Microsoft.Extensions.Caching.Memory;

using Moq;
using Moq.Protected;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordPwnedCheckerTests
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<PasswordPwnedChecker> _logger;

    public PasswordPwnedCheckerTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<PasswordPwnedChecker>();
    }

    [Fact]
    public async Task IsPwnedAsync_ReturnsTrue_WhenPasswordIsPwned()
    {
        // Arrange
        // "password" SHA-1: 5BAA61E4C9B93F3F0682250B6CF8331B7EE68FD8
        // Prefix: 5BAA6, Suffix: 1E4C9B93F3F0682250B6CF8331B7EE68FD8
        var mockHttp = CreateMockHttpClient("1E4C9B93F3F0682250B6CF8331B7EE68FD8:3730471");
        var checker = new PasswordPwnedChecker(mockHttp, _cache, _logger);

        // Act
        var isPwned = await checker.IsPwnedAsync("password", CancellationToken.None);

        // Assert
        Assert.True(isPwned);
    }

    [Fact]
    public async Task IsPwnedAsync_ReturnsFalse_WhenPasswordIsNotPwned()
    {
        // Arrange
        var mockHttp = CreateMockHttpClient("AAAAA:1\nBBBBB:2");
        var checker = new PasswordPwnedChecker(mockHttp, _cache, _logger);

        // Act
        var isPwned = await checker.IsPwnedAsync("SuperSecureP@ssw0rd!XYZ123", CancellationToken.None);

        // Assert
        Assert.False(isPwned);
    }

    [Fact]
    public async Task GetPwnedCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var mockHttp = CreateMockHttpClient("1E4C9B93F3F0682250B6CF8331B7EE68FD8:123456");
        var checker = new PasswordPwnedChecker(mockHttp, _cache, _logger);

        // Act
        var count = await checker.GetPwnedCountAsync("password", CancellationToken.None);

        // Assert
        Assert.Equal(123456, count);
    }

    [Fact]
    public async Task GetPwnedCountAsync_ReturnsZero_WhenPasswordIsEmpty()
    {
        // Arrange
        var mockHttp = CreateMockHttpClient("");
        var checker = new PasswordPwnedChecker(mockHttp, _cache, _logger);

        // Act
        var count = await checker.GetPwnedCountAsync("", CancellationToken.None);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetPwnedCountAsync_UsesCacheOnSecondCall()
    {
        // Arrange
        var callCount = 0;
        var cache = new MemoryCache(new MemoryCacheOptions());
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("1E4C9B93F3F0682250B6CF8331B7EE68FD8:999")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new PasswordPwnedChecker(httpClient, cache, _logger);

        // Act
        var count1 = await checker.GetPwnedCountAsync("password", CancellationToken.None);
        var count2 = await checker.GetPwnedCountAsync("password", CancellationToken.None);

        // Assert
        Assert.Equal(999, count1);
        Assert.Equal(999, count2);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetPwnedCountAsync_ReturnsZero_OnHttpError()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object);
        var checker = new PasswordPwnedChecker(httpClient, _cache, _logger);

        // Act
        var count = await checker.GetPwnedCountAsync("password", CancellationToken.None);

        // Assert
        Assert.Equal(0, count);
    }

    private static HttpClient CreateMockHttpClient(string responseContent)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        return new HttpClient(mockHandler.Object);
    }
}
