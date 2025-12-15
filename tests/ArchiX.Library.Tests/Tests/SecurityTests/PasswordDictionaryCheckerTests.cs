using ArchiX.Library.Runtime.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordDictionaryCheckerTests
{
    private readonly IMemoryCache _cache;
    private readonly PasswordDictionaryChecker _checker;

    public PasswordDictionaryCheckerTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _checker = new PasswordDictionaryChecker(_cache, NullLogger<PasswordDictionaryChecker>.Instance);
    }

    [Fact]
    public async Task IsCommonPasswordAsync_ReturnsTrue_WhenPasswordIsCommon()
    {
        var result = await _checker.IsCommonPasswordAsync("password");

        Assert.True(result);
    }

    [Fact]
    public async Task IsCommonPasswordAsync_ReturnsFalse_WhenPasswordIsNotCommon()
    {
        var result = await _checker.IsCommonPasswordAsync("UniqueP@ssw0rd!9876");

        Assert.False(result);
    }

    [Fact]
    public async Task IsCommonPasswordAsync_CaseInsensitive_ReturnsTrue()
    {
        var result1 = await _checker.IsCommonPasswordAsync("PASSWORD");
        var result2 = await _checker.IsCommonPasswordAsync("PaSsWoRd");

        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public async Task IsCommonPasswordAsync_ReturnsFalse_WhenPasswordIsEmpty()
    {
        var result = await _checker.IsCommonPasswordAsync("");

        Assert.False(result);
    }

    [Fact]
    public async Task IsCommonPasswordAsync_UsesCacheOnSecondCall()
    {
        await _checker.IsCommonPasswordAsync("qwerty");
        var result = await _checker.IsCommonPasswordAsync("123456");

        Assert.True(result);
    }

    [Fact]
    public async Task GetDictionaryWordCount_ReturnsPositiveNumber()
    {
        await _checker.IsCommonPasswordAsync("test");
        var count = _checker.GetDictionaryWordCount();

        Assert.True(count > 0);
    }

    [Fact]
    public async Task IsCommonPasswordAsync_MultipleCommonPasswords_ReturnsTrue()
    {
        var commonPasswords = new[] { "123456", "qwerty", "admin", "letmein", "welcome" };

        foreach (var pwd in commonPasswords)
        {
            var result = await _checker.IsCommonPasswordAsync(pwd);
            Assert.True(result, $"'{pwd}' should be in dictionary");
        }
    }

    [Fact]
    public async Task IsCommonPasswordAsync_StrongPasswords_ReturnsFalse()
    {
        var strongPasswords = new[]
        {
            "GoodP@ssw0rd!123",
            "MyStr0ng!Pass#2024",
            "C0mpl3x$Secur1ty"
        };

        foreach (var pwd in strongPasswords)
        {
            var result = await _checker.IsCommonPasswordAsync(pwd);
            Assert.False(result, $"'{pwd}' should NOT be in dictionary");
        }
    }
}