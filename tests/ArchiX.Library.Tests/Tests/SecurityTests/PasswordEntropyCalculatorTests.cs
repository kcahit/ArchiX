using ArchiX.Library.Runtime.Security;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordEntropyCalculatorTests
{
    private readonly PasswordEntropyCalculator _calculator = new();

    [Fact]
    public void CalculateEntropy_EmptyString_ReturnsZero()
    {
        var result = _calculator.CalculateEntropy(string.Empty);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void CalculateEntropy_SingleChar_ReturnsZero()
    {
        // Single char repeated = no entropy
        var result = _calculator.CalculateEntropy("aaaa");
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void CalculateEntropy_TwoDistinctChars_ReturnsOne()
    {
        // "ab" = 2 chars, equal distribution = 1 bit entropy
        var result = _calculator.CalculateEntropy("ab");
        Assert.Equal(1.0, result, precision: 2);
    }

    [Fact]
    public void CalculateEntropy_WeakPassword_ReturnsLowEntropy()
    {
        // "password" - yaygýn kelime, düþük entropy
        var result = _calculator.CalculateEntropy("password");
        Assert.True(result < 3.0, $"Weak password entropy should be < 3.0, got {result}");
    }

    [Fact]
    public void CalculateEntropy_StrongPassword_ReturnsHighEntropy()
    {
        // "A1!xY9#z" - karýþýk karakterler, yüksek entropy
        var result = _calculator.CalculateEntropy("A1!xY9#z");
        Assert.True(result >= 2.5, $"Strong password entropy should be >= 2.5, got {result}");
    }

    [Fact]
    public void CalculateTotalEntropy_EmptyString_ReturnsZero()
    {
        var result = _calculator.CalculateTotalEntropy(string.Empty);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void CalculateTotalEntropy_MultipleChars_ReturnsProduct()
    {
        var password = "A1!xY9#z";
        var perChar = _calculator.CalculateEntropy(password);
        var total = _calculator.CalculateTotalEntropy(password);
        
        Assert.Equal(perChar * password.Length, total, precision: 2);
    }

    [Fact]
    public void MeetsMinimumEntropy_BelowThreshold_ReturnsFalse()
    {
        var result = _calculator.MeetsMinimumEntropy("password", minEntropyBits: 30.0);
        Assert.False(result);
    }

    [Fact]
    public void MeetsMinimumEntropy_AboveThreshold_ReturnsTrue()
    {
        var result = _calculator.MeetsMinimumEntropy("A1!xY9#zK2@wQ5$", minEntropyBits: 30.0);
        Assert.True(result);
    }

    [Fact]
    public void MeetsMinimumEntropy_ZeroThreshold_ReturnsTrue()
    {
        // Threshold = 0 means disabled
        var result = _calculator.MeetsMinimumEntropy("weak", minEntropyBits: 0.0);
        Assert.True(result);
    }

    [Fact]
    public void MeetsMinimumEntropy_NegativeThreshold_ReturnsTrue()
    {
        // Negative threshold means disabled
        var result = _calculator.MeetsMinimumEntropy("weak", minEntropyBits: -1.0);
        Assert.True(result);
    }

    [Fact]
    public void CalculateEntropy_AllDistinctChars_ReturnsMaxEntropy()
    {
        var password = "abcdefgh"; // 8 distinct chars
        var result = _calculator.CalculateEntropy(password);
        
        // Max entropy for 8 distinct chars = log2(8) = 3.0
        Assert.Equal(3.0, result, precision: 2);
    }

    [Theory]
    [InlineData("aaa", 0.0)]
    [InlineData("abc", 1.58)] // log2(3) ? 1.58
    [InlineData("abcd", 2.0)] // log2(4) = 2.0
    [InlineData("P@ssw0rd!123", 3.0)] // Mixed chars
    public void CalculateEntropy_VariousPasswords_ReturnsExpected(string password, double expectedMin)
    {
        var result = _calculator.CalculateEntropy(password);
        Assert.True(result >= expectedMin - 0.5, $"Expected >= {expectedMin}, got {result}");
    }
}