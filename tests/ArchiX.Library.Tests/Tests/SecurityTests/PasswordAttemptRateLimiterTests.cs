using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Runtime.Security;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordAttemptRateLimiterTests
{
    private readonly IMemoryCache _cache;
    private readonly Mock<IPasswordPolicyProvider> _policyProvider;
    private readonly ILogger<PasswordAttemptRateLimiter> _logger;
    private PasswordPolicyOptions _policy;

    public PasswordAttemptRateLimiterTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _policyProvider = new Mock<IPasswordPolicyProvider>();
        _logger = NullLogger<PasswordAttemptRateLimiter>.Instance;

        _policy = new PasswordPolicyOptions
        {
            LockoutThreshold = 5,
            LockoutSeconds = 300
        };

        _policyProvider
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _policy);
    }

    private PasswordAttemptRateLimiter CreateService() =>
        new(_cache, _policyProvider.Object, _logger);

    [Fact]
    public async Task IsRateLimitExceededAsync_NoAttempts_ReturnsFalse()
    {
        var service = CreateService();
        var result = await service.IsRateLimitExceededAsync("user:123");

        Assert.False(result);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_BelowThreshold_ReturnsFalse()
    {
        var service = CreateService();

        for (var i = 0; i < 4; i++)
            await service.RecordAttemptAsync("user:123");

        var result = await service.IsRateLimitExceededAsync("user:123");

        Assert.False(result);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_AtThreshold_ReturnsTrue()
    {
        var service = CreateService();

        for (var i = 0; i < 5; i++)
            await service.RecordAttemptAsync("user:123");

        var result = await service.IsRateLimitExceededAsync("user:123");

        Assert.True(result);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_ExceedsThreshold_ReturnsTrue()
    {
        var service = CreateService();

        for (var i = 0; i < 10; i++)
            await service.RecordAttemptAsync("user:123");

        var result = await service.IsRateLimitExceededAsync("user:123");

        Assert.True(result);
    }

    [Fact]
    public async Task RecordAttemptAsync_StoresTimestamp()
    {
        var service = CreateService();

        await service.RecordAttemptAsync("user:123");
        var (remaining, _) = await service.GetStatusAsync("user:123");

        Assert.Equal(4, remaining);
    }

    [Fact]
    public async Task RecordAttemptAsync_MultipleUsers_Isolated()
    {
        var service = CreateService();

        for (var i = 0; i < 5; i++)
            await service.RecordAttemptAsync("user:123");

        await service.RecordAttemptAsync("user:456");

        var result1 = await service.IsRateLimitExceededAsync("user:123");
        var result2 = await service.IsRateLimitExceededAsync("user:456");

        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public async Task ResetAsync_ClearsAttempts()
    {
        var service = CreateService();

        for (var i = 0; i < 5; i++)
            await service.RecordAttemptAsync("user:123");

        await service.ResetAsync("user:123");

        var result = await service.IsRateLimitExceededAsync("user:123");
        Assert.False(result);
    }

    [Fact]
    public async Task GetStatusAsync_NoAttempts_ReturnsMaxRemaining()
    {
        var service = CreateService();

        var (remaining, retryAfter) = await service.GetStatusAsync("user:123");

        Assert.Equal(5, remaining);
        Assert.Equal(0, retryAfter);
    }

    [Fact]
    public async Task GetStatusAsync_SomeAttempts_ReturnsCorrectRemaining()
    {
        var service = CreateService();

        for (var i = 0; i < 3; i++)
            await service.RecordAttemptAsync("user:123");

        var (remaining, retryAfter) = await service.GetStatusAsync("user:123");

        Assert.Equal(2, remaining);
        Assert.Equal(0, retryAfter);
    }

    [Fact]
    public async Task GetStatusAsync_ExceededLimit_ReturnsRetryAfter()
    {
        var service = CreateService();

        for (var i = 0; i < 5; i++)
            await service.RecordAttemptAsync("user:123");

        var (remaining, retryAfter) = await service.GetStatusAsync("user:123");

        Assert.Equal(0, remaining);
        Assert.True(retryAfter > 0);
        Assert.True(retryAfter <= 300);
    }

    [Fact]
    public async Task SlidingWindow_ExpiredAttemptsIgnored()
    {
        _policy = _policy with { LockoutSeconds = 2 };

        var service = CreateService();

        await service.RecordAttemptAsync("user:123");
        await Task.Delay(2500);

        var result = await service.IsRateLimitExceededAsync("user:123");

        Assert.False(result);
    }

    [Fact]
    public async Task CustomPolicy_UsesCorrectThreshold()
    {
        _policy = _policy with { LockoutThreshold = 3 };

        var service = CreateService();

        for (var i = 0; i < 3; i++)
            await service.RecordAttemptAsync("user:123");

        var result = await service.IsRateLimitExceededAsync("user:123");

        Assert.True(result);
    }

    [Fact]
    public async Task ZeroThreshold_UsesFallbackDefault()
    {
        _policy = _policy with { LockoutThreshold = 0 };

        var service = CreateService();

        for (var i = 0; i < 5; i++)
            await service.RecordAttemptAsync("user:123");

        var result = await service.IsRateLimitExceededAsync("user:123");

        Assert.True(result);
    }

    [Fact]
    public async Task EmptyKey_ThrowsException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.IsRateLimitExceededAsync(string.Empty));
    }

    [Fact]
    public async Task NullKey_ThrowsException()
    {
        var service = CreateService();

        // ✅ ArgumentNullException bekleniyor (ThrowIfNullOrWhiteSpace null için bunu fırlatır)
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.IsRateLimitExceededAsync(null!));
    }
}
