using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;

using Moq;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordValidationServiceTests
{
    private readonly Mock<IPasswordPolicyProvider> _policyProvider = new();
    private readonly Mock<IPasswordPwnedChecker> _pwnedChecker = new();
    private readonly Mock<IPasswordHistoryService> _historyService = new();
    private readonly Mock<IPasswordBlacklistService> _blacklistService = new();
    private readonly Mock<IPasswordDictionaryChecker> _dictionaryChecker = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IPasswordExpirationService> _expirationService = new();
    private readonly Mock<IPasswordEntropyCalculator> _entropyCalculator = new();
    private readonly Mock<IDbContextFactory<AppDbContext>> _dbContextFactory = new();
    private PasswordPolicyOptions _policy;

    public PasswordValidationServiceTests()
    {
        _policy = new PasswordPolicyOptions
        {
            Version = 1,
            MinLength = 12,
            MaxLength = 128,
            RequireUpper = true,
            RequireLower = true,
            RequireDigit = true,
            RequireSymbol = true,
            AllowedSymbols = "!@#$%^&*_-+=:?.,;",
            MinDistinctChars = 5,
            MaxRepeatedSequence = 3,
            BlockList = [],
            HistoryCount = 10,
            EnableDictionaryCheck = true,
            Hash = new PasswordHashOptions
            {
                Algorithm = "Argon2id",
                MemoryKb = 65536,
                Parallelism = 2,
                Iterations = 3
            }
        };

        _policyProvider
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _policy);

        _blacklistService
            .Setup(x => x.IsWordBlockedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dictionaryChecker
            .Setup(x => x.IsCommonPasswordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _pwnedChecker
            .Setup(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _historyService
            .Setup(x => x.IsPasswordInHistoryAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hasher
            .Setup(x => x.HashAsync(It.IsAny<string>(), It.IsAny<PasswordPolicyOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string pwd, PasswordPolicyOptions _, CancellationToken _) => $"HASHED_{pwd}");

        _entropyCalculator
            .Setup(x => x.MeetsMinimumEntropy(It.IsAny<string>(), It.IsAny<double>()))
            .Returns(true);
    }

    private PasswordValidationService CreateService() => new(
        _policyProvider.Object,
        _pwnedChecker.Object,
        _historyService.Object,
        _blacklistService.Object,
        _dictionaryChecker.Object,
        _hasher.Object,
        _expirationService.Object,
        _entropyCalculator.Object,
        _dbContextFactory.Object);

    [Fact]
    public async Task ValidateAsync_ValidPassword_ReturnsSuccess()
    {
        var service = CreateService();
        var result = await service.ValidateAsync("GoodP@ssw0rd!123", userId: 1);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_TooShort_ReturnsMinLengthError()
    {
        var service = CreateService();
        var result = await service.ValidateAsync("Short1!", userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("MIN_LENGTH", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_DynamicBlacklist_ReturnsDynamicBlockError()
    {
        _blacklistService
            .Setup(x => x.IsWordBlockedAsync("BadWord12345!", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.ValidateAsync("BadWord12345!", userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("DYNAMIC_BLOCK", result.Errors);
        _dictionaryChecker.Verify(x => x.IsCommonPasswordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _pwnedChecker.Verify(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_DictionaryWord_ReturnsDictionaryWordError()
    {
        _dictionaryChecker
            .Setup(x => x.IsCommonPasswordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.ValidateAsync("Password123!", userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("DICTIONARY_WORD", result.Errors);
        _pwnedChecker.Verify(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_DictionaryCheckDisabled_SkipsDictionaryCheck()
    {
        _policy = _policy with { EnableDictionaryCheck = false };
        _dictionaryChecker
            .Setup(x => x.IsCommonPasswordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.ValidateAsync("GoodP@ssw0rd!123", userId: 1);

        Assert.True(result.IsValid);
        _dictionaryChecker.Verify(x => x.IsCommonPasswordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_Pwned_ReturnsPwnedError()
    {
        _pwnedChecker
            .Setup(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.ValidateAsync("GoodP@ssw0rd!123", userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("PWNED", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InHistory_ReturnsHistoryError()
    {
        _historyService
            .Setup(x => x.IsPasswordInHistoryAsync(1, "HASHED_GoodP@ssw0rd!123", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.ValidateAsync("GoodP@ssw0rd!123", userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("HISTORY", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_PolicyErrorsStopsPwnedCheck()
    {
        var service = CreateService();
        var result = await service.ValidateAsync("short", userId: 1);

        Assert.False(result.IsValid);
        _pwnedChecker.Verify(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_ExpiredPassword_ReturnsExpiredError()
    {
        _policy = _policy with { MaxPasswordAgeDays = 30 };
        var (context, userId) = CreateContextWithUser(DateTimeOffset.UtcNow.AddDays(-60));

        _dbContextFactory
            .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken _) => Task.FromResult(context));

        _expirationService
            .Setup(x => x.IsExpired(It.IsAny<User>(), It.Is<PasswordPolicyOptions>(p => p.MaxPasswordAgeDays == 30), It.IsAny<DateTimeOffset?>()))
            .Returns(true);

        var service = CreateService();
        var result = await service.ValidateAsync("GoodP@ssw0rd!123", userId);

        Assert.False(result.IsValid);
        Assert.Contains("EXPIRED", result.Errors);
        _blacklistService.Verify(x => x.IsWordBlockedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_LowEntropy_ReturnsLowEntropyError()
    {
        _policy = _policy with { MinEntropyBits = 50.0 };
        _entropyCalculator
            .Setup(x => x.MeetsMinimumEntropy(It.IsAny<string>(), 50.0))
            .Returns(false);

        var service = CreateService();
        var result = await service.ValidateAsync("GoodP@ssw0rd!123", userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("LOW_ENTROPY", result.Errors);
    }

    private static (AppDbContext Context, int UserId) CreateContextWithUser(DateTimeOffset? passwordChangedAt)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"PwdValidation_{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        var user = new User
        {
            UserName = "tester",
            NormalizedUserName = "TESTER",
            StatusId = BaseEntity.ApprovedStatusId,
            PasswordChangedAtUtc = passwordChangedAt
        };

        context.Users.Add(user);
        context.SaveChanges();
        return (context, user.Id);
    }
}
