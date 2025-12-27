using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordExpirationServiceTests
{
    private readonly PasswordExpirationService _service = new();

    [Fact]
    public void IsExpired_ReturnsFalse_WhenMaxAgeIsNull()
    {
        var policy = CreatePolicy(null);
        var result = _service.IsExpired(CreateUser(DateTimeOffset.UtcNow), policy);
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenPasswordChangedIsNull()
    {
        var policy = CreatePolicy(30);
        var result = _service.IsExpired(CreateUser(null), policy);
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_WhenPasswordStillValid()
    {
        var policy = CreatePolicy(30);
        var user = CreateUser(DateTimeOffset.UtcNow.AddDays(-10));
        var result = _service.IsExpired(user, policy, DateTimeOffset.UtcNow);
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenPasswordExpired()
    {
        var policy = CreatePolicy(30);
        var user = CreateUser(DateTimeOffset.UtcNow.AddDays(-60));
        var result = _service.IsExpired(user, policy, DateTimeOffset.UtcNow);
        Assert.True(result);
    }

    [Fact]
    public void IsExpired_Throws_WhenMaxAgeInvalid()
    {
        var policy = CreatePolicy(0);
        var user = CreateUser(DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => _service.IsExpired(user, policy));
    }

    [Fact]
    public void GetDaysUntilExpiration_ReturnsNull_WhenMaxAgeNull()
    {
        var policy = CreatePolicy(null);
        var result = _service.GetDaysUntilExpiration(CreateUser(DateTimeOffset.UtcNow), policy);
        Assert.Null(result);
    }

    [Fact]
    public void GetDaysUntilExpiration_ReturnsNull_WhenPasswordChangedNull()
    {
        var policy = CreatePolicy(30);
        var result = _service.GetDaysUntilExpiration(CreateUser(null), policy);
        Assert.Null(result);
    }

    [Fact]
    public void GetDaysUntilExpiration_ReturnsZero_WhenExpired()
    {
        var policy = CreatePolicy(30);
        var user = CreateUser(DateTimeOffset.UtcNow.AddDays(-60));
        var result = _service.GetDaysUntilExpiration(user, policy, DateTimeOffset.UtcNow);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetDaysUntilExpiration_ReturnsCorrectValue()
    {
        var policy = CreatePolicy(30);
        var user = CreateUser(DateTimeOffset.UtcNow.AddDays(-10));
        var result = _service.GetDaysUntilExpiration(user, policy, DateTimeOffset.UtcNow);
        Assert.Equal(20, result);
    }

    [Fact]
    public void GetExpirationDate_ReturnsNull_WhenPolicyNull()
    {
        var result = _service.GetExpirationDate(CreateUser(DateTimeOffset.UtcNow), null!);
        Assert.Null(result);
    }

    [Fact]
    public void GetExpirationDate_ReturnsNull_WhenPasswordChangedNull()
    {
        var policy = CreatePolicy(30);
        var result = _service.GetExpirationDate(CreateUser(null), policy);
        Assert.Null(result);
    }

    [Fact]
    public void GetExpirationDate_ReturnsCorrectDate()
    {
        var changedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var policy = CreatePolicy(30);
        var result = _service.GetExpirationDate(CreateUser(changedAt), policy);
        Assert.Equal(changedAt.AddDays(30), result);
    }

    private static PasswordPolicyOptions CreatePolicy(int? maxAgeDays) => new()
    {
        MaxPasswordAgeDays = maxAgeDays,
        MinLength = 12,
        MaxLength = 128,
        Hash = new PasswordHashOptions()
    };

    private static User CreateUser(DateTimeOffset? changedAt) => new()
    {
        UserName = "tester",
        NormalizedUserName = "TESTER",
        StatusId = BaseEntity.ApprovedStatusId,
        PasswordChangedAtUtc = changedAt
    };
}
