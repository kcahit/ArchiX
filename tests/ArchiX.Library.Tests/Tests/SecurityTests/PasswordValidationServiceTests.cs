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
    private readonly Mock<IPasswordHasher> _hasher;
    private readonly ILogger<PasswordValidationService> _logger;

    public PasswordValidationServiceTests()
    {
        _policyProvider = new Mock<IPasswordPolicyProvider>();
        _pwnedChecker = new Mock<IPasswordPwnedChecker>();
        _historyService = new Mock<IPasswordHistoryService>();
        _hasher = new Mock<IPasswordHasher>();
        _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<PasswordValidationService>();

        // Default policy (tüm kontroller açık)
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
                LockoutThreshold = 5,
                LockoutSeconds = 900,
                Hash = new PasswordHashOptions
                {
                    Algorithm = "Argon2id",
                    MemoryKb = 65536,
                    Parallelism = 2,
                    Iterations = 3,
                    SaltLength = 16,
                    HashLength = 32,
                    PepperEnabled = false,
                    Fallback = new PasswordHashFallbackOptions
                    {
                        Algorithm = "PBKDF2-SHA512",
                        Iterations = 210000
                    }
                }
            });

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
            _hasher.Object,
            _logger);
    }

    [Fact]
    public async Task ValidateAsync_ValidPassword_ReturnsSuccess()
    {
        // Arrange
        var service = CreateService();
        var password = "GoodP@ssw0rd!123";

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_TooShort_ReturnsMinLengthError()
    {
        // Arrange
        var service = CreateService();
        var password = "Short1!"; // < 12 karakter

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("MIN_LENGTH", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_MissingUppercase_ReturnsReqUpperError()
    {
        // Arrange
        var service = CreateService();
        var password = "goodp@ssw0rd!123"; // Büyük harf yok

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("REQ_UPPER", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_Pwned_ReturnsPwnedError()
    {
        // Arrange
        _pwnedChecker
            .Setup(x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // ✅ PWNED!

        var service = CreateService();
        var password = "GoodP@ssw0rd!123";

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("PWNED", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InHistory_ReturnsHistoryError()
    {
        // Arrange
        _historyService
            .Setup(x => x.IsPasswordInHistoryAsync(1, "HASHED_GoodP@ssw0rd!123", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // ✅ Geçmişte kullanılmış!

        var service = CreateService();
        var password = "GoodP@ssw0rd!123";

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("HISTORY", result.Errors);

        // Hash metodunun çağrıldığını doğrula
        _hasher.Verify(x => x.HashAsync(password, It.IsAny<PasswordPolicyOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_HistoryCountZero_SkipsHistoryCheck()
    {
        // Arrange
        _policyProvider
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordPolicyOptions
            {
                MinLength = 12,
                MaxLength = 128,
                RequireUpper = true,
                RequireLower = true,
                RequireDigit = true,
                RequireSymbol = true,
                HistoryCount = 0, // ✅ History kapalı
                Hash = new PasswordHashOptions()
            });

        var service = CreateService();
        var password = "GoodP@ssw0rd!123";

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.True(result.IsValid);

        // History servisi ÇAĞRILMAMALI
        _historyService.Verify(
            x => x.IsPasswordInHistoryAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var service = CreateService();
        var password = "short"; // MIN_LENGTH, REQ_UPPER, REQ_DIGIT, REQ_SYMBOL

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("MIN_LENGTH", result.Errors);
        Assert.Contains("REQ_UPPER", result.Errors);
        Assert.Contains("REQ_DIGIT", result.Errors);
        Assert.Contains("REQ_SYMBOL", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_PolicyErrorsStopsPwnedCheck()
    {
        // Arrange
        var service = CreateService();
        var password = "short"; // Policy hatası var

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);

        // Pwned checker ÇAĞRILMAMALI (performans optimizasyonu)
        _pwnedChecker.Verify(
            x => x.IsPwnedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_BlockedWord_ReturnsBlockListError()
    {
        // Arrange
        var service = CreateService();
        var password = "MyPassword123!"; // "password" kelimesini içeriyor

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("BLOCK_LIST", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_RepeatedSequence_ReturnsRepeatSeqError()
    {
        // Arrange
        var service = CreateService();
        var password = "GoodPaaaa@ssw0rd!"; // 'aaaa' (4 tekrar > maxRepeatedSequence:3)

        // Act
        var result = await service.ValidateAsync(password, userId: 1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("REPEAT_SEQ", result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_CallsHashWithCorrectPolicy()
    {
        // Arrange
        var policy = new PasswordPolicyOptions { MinLength = 12, HistoryCount = 10, Hash = new PasswordHashOptions() };
        _policyProvider
            .Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var service = CreateService();
        var password = "GoodP@ssw0rd!123";

        // Act
        await service.ValidateAsync(password, userId: 1, applicationId: 1);

        // Assert
        _hasher.Verify(x => x.HashAsync(password, policy, It.IsAny<CancellationToken>()), Times.Once);
    }
}
