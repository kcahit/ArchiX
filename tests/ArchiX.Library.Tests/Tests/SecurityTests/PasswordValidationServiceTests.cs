using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Runtime.Security;

using Moq;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordValidationServiceTests
{
    private readonly Mock<IPasswordPolicyProvider> _policyProvider;
    private readonly Mock<IPasswordPwnedChecker> _pwnedChecker;
    private readonly Mock<IPasswordHistoryService> _historyService;
    private readonly Mock<IPasswordBlacklistService> _blacklistService;
    private readonly Mock<IPasswordHasher> _hasher;

    public PasswordValidationServiceTests()
    {
        _policyProvider = new Mock<IPasswordPolicyProvider>();
        _pwnedChecker = new Mock<IPasswordPwnedChecker>();
        _historyService = new Mock<IPasswordHistoryService>();
        _blacklistService = new Mock<IPasswordBlacklistService>();
        _hasher = new Mock<IPasswordHasher>();

        // Default policy
        _policyProvider
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordPolicyOptions
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
                BlockList = new[] { "password", "123456", "qwerty", "admin" },
                HistoryCount = 10,
                Hash = new PasswordHashOptions
                {
                    Algorithm = "Argon2id",
                    MemoryKb = 65536,
                    Parallelism = 2,
                    Iterations = 3
                }
            });

        // Default: Blacklist kontrolü false
        _blacklistService
            .Setup(x => x.IsWordBlockedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Default: Pwned kontrolü false
        _pwnedChecker
            .Setup(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Default: History kontrolü false
        _historyService
            .Setup(x => x.IsPasswordInHistoryAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Default: Hash mock
        _hasher
            .Setup(x => x.HashAsync(It.IsAny<string>(), It.IsAny<PasswordPolicyOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string pwd, PasswordPolicyOptions policy, CancellationToken ct) => $"HASHED_{pwd}");
    }

    private PasswordValidationService CreateService()
    {
        return new PasswordValidationService(
            _policyProvider.Object,
            _pwnedChecker.Object,
            _historyService.Object,
            _blacklistService.Object,
            _hasher.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidPassword_ReturnsSuccess()
    {
        var service = CreateService();
        var password = "GoodP@ssw0rd!123";

        var result = await service.ValidateAsync(password, userId: 1);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_TooShort_ReturnsMinLengthError()
    {
        var service = CreateService();
        var password = "Short1!";

        var result = await service.ValidateAsync(password, userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("MIN_LENGTH", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_Blacklisted_ReturnsBlacklistError()
    {
        _blacklistService
            .Setup(x => x.IsWordBlockedAsync("BadWord12345!", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var password = "BadWord12345!"; // ✅ 13 karakter (policy geçer)

        var result = await service.ValidateAsync(password, userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("BLACKLIST", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_Pwned_ReturnsPwnedError()
    {
        _pwnedChecker
            .Setup(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var password = "GoodP@ssw0rd!123";

        var result = await service.ValidateAsync(password, userId: 1);

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
        var password = "GoodP@ssw0rd!123";

        var result = await service.ValidateAsync(password, userId: 1);

        Assert.False(result.IsValid);
        Assert.Contains("HISTORY", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_PolicyErrorsStopsPwnedCheck()
    {
        var service = CreateService();
        var password = "short";

        var result = await service.ValidateAsync(password, userId: 1);

        Assert.False(result.IsValid);
        _pwnedChecker.Verify(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
