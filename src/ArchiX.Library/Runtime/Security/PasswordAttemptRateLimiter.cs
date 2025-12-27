using System.Collections.Concurrent;

using ArchiX.Library.Abstractions.Security;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

internal sealed class PasswordAttemptRateLimiter : IPasswordAttemptRateLimiter
{
    private readonly IMemoryCache _cache;
    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly ILogger<PasswordAttemptRateLimiter> _logger;

    private const string CacheKeyPrefix = "pwd:ratelimit:";
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromMinutes(5);
    private const int DefaultMaxAttempts = 5;

    public PasswordAttemptRateLimiter(
        IMemoryCache cache,
        IPasswordPolicyProvider policyProvider,
        ILogger<PasswordAttemptRateLimiter> logger)
    {
        _cache = cache;
        _policyProvider = policyProvider;
        _logger = logger;
    }

    public async Task<bool> IsRateLimitExceededAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = BuildCacheKey(key);
        var attempts = GetAttempts(cacheKey);

        if (attempts.Count == 0)
            return false;

        var policy = await _policyProvider.GetAsync(applicationId: 1, cancellationToken).ConfigureAwait(false);
        var maxAttempts = policy.LockoutThreshold > 0 ? policy.LockoutThreshold : DefaultMaxAttempts;
        var windowSeconds = policy.LockoutSeconds > 0 ? policy.LockoutSeconds : (int)DefaultWindow.TotalSeconds;
        var window = TimeSpan.FromSeconds(windowSeconds);

        var now = DateTimeOffset.UtcNow;
        var validAttempts = attempts
            .Where(timestamp => now - timestamp < window)
            .ToList();

        var isExceeded = validAttempts.Count >= maxAttempts;

        if (isExceeded)
        {
            _logger.LogWarning(
                "Rate limit exceeded for key {Key}. Attempts: {Count}/{Max} within {Window}s",
                key,
                validAttempts.Count,
                maxAttempts,
                windowSeconds);
        }

        return isExceeded;
    }

    public async Task RecordAttemptAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = BuildCacheKey(key);
        var attempts = GetAttempts(cacheKey);

        var now = DateTimeOffset.UtcNow;
        attempts.Add(now);

        var policy = await _policyProvider.GetAsync(applicationId: 1, cancellationToken).ConfigureAwait(false);
        var windowSeconds = policy.LockoutSeconds > 0 ? policy.LockoutSeconds : (int)DefaultWindow.TotalSeconds;
        var window = TimeSpan.FromSeconds(windowSeconds);

        var validAttempts = attempts
            .Where(timestamp => now - timestamp < window)
            .ToList();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(window)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, new ConcurrentBag<DateTimeOffset>(validAttempts), cacheOptions);

        _logger.LogDebug(
            "Recorded attempt for key {Key}. Total valid attempts: {Count}",
            key,
            validAttempts.Count);
    }

    public Task ResetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = BuildCacheKey(key);
        _cache.Remove(cacheKey);

        _logger.LogInformation("Rate limit reset for key {Key}", key);

        return Task.CompletedTask;
    }

    public async Task<(int RemainingAttempts, int RetryAfterSeconds)> GetStatusAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = BuildCacheKey(key);
        var attempts = GetAttempts(cacheKey);

        var policy = await _policyProvider.GetAsync(applicationId: 1, cancellationToken).ConfigureAwait(false);
        var maxAttempts = policy.LockoutThreshold > 0 ? policy.LockoutThreshold : DefaultMaxAttempts;
        var windowSeconds = policy.LockoutSeconds > 0 ? policy.LockoutSeconds : (int)DefaultWindow.TotalSeconds;
        var window = TimeSpan.FromSeconds(windowSeconds);

        var now = DateTimeOffset.UtcNow;
        var validAttempts = attempts
            .Where(timestamp => now - timestamp < window)
            .OrderBy(t => t)
            .ToList();

        var remaining = Math.Max(0, maxAttempts - validAttempts.Count);

        var retryAfter = 0;
        if (validAttempts.Count >= maxAttempts && validAttempts.Count > 0)
        {
            var oldestAttempt = validAttempts.First();
            var windowEnd = oldestAttempt.Add(window);
            retryAfter = (int)Math.Ceiling((windowEnd - now).TotalSeconds);
            retryAfter = Math.Max(0, retryAfter);
        }

        return (remaining, retryAfter);
    }

    private static string BuildCacheKey(string key) => $"{CacheKeyPrefix}{key}";

    private ConcurrentBag<DateTimeOffset> GetAttempts(string cacheKey)
    {
        if (_cache.TryGetValue<ConcurrentBag<DateTimeOffset>>(cacheKey, out var existing) && existing is not null)
            return existing;

        return new ConcurrentBag<DateTimeOffset>();
    }
}
