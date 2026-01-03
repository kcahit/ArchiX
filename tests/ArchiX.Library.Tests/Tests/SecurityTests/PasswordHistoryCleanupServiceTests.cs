using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public class PasswordHistoryCleanupServiceTests
{
    private readonly Mock<IPasswordPolicyProvider> _mockPolicyProvider;
    private AppDbContext _context = null!;
    private PasswordHistoryCleanupService _service = null!;

    public PasswordHistoryCleanupServiceTests()
    {
        _mockPolicyProvider = new Mock<IPasswordPolicyProvider>();
    }

    private void InitializeInMemoryDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _service = new PasswordHistoryCleanupService(
            _context,
            _mockPolicyProvider.Object,
            NullLogger<PasswordHistoryCleanupService>.Instance);
    }

    [Fact]
    public async Task CleanupUserHistoryAsync_KeepCountZero_ReturnsZero()
    {
        InitializeInMemoryDatabase();

        var result = await _service.CleanupUserHistoryAsync(userId: 1, keepCount: 0);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CleanupUserHistoryAsync_NoHistory_ReturnsZero()
    {
        InitializeInMemoryDatabase();

        var result = await _service.CleanupUserHistoryAsync(userId: 999, keepCount: 5);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CleanupUserHistoryAsync_LessThanKeepCount_ReturnsZero()
    {
        InitializeInMemoryDatabase();

        _context.UserPasswordHistories.AddRange(
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash1", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash2", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash3", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1) }
        );
        await _context.SaveChangesAsync();

        var result = await _service.CleanupUserHistoryAsync(userId: 1, keepCount: 5);

        Assert.Equal(0, result);
        Assert.Equal(3, await _context.UserPasswordHistories.CountAsync(h => h.UserId == 1));
    }

    [Fact]
    public async Task CleanupUserHistoryAsync_RemovesOldest_KeepsMostRecent()
    {
        InitializeInMemoryDatabase();

        _context.UserPasswordHistories.AddRange(
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash1", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-5) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash2", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-4) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash3", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash4", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "hash5", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1) }
        );
        await _context.SaveChangesAsync();

        var result = await _service.CleanupUserHistoryAsync(userId: 1, keepCount: 3);

        Assert.Equal(2, result);
        var remaining = await _context.UserPasswordHistories.Where(h => h.UserId == 1).ToListAsync();
        Assert.Equal(3, remaining.Count);
        Assert.Contains(remaining, h => h.PasswordHash == "hash3");
        Assert.Contains(remaining, h => h.PasswordHash == "hash4");
        Assert.Contains(remaining, h => h.PasswordHash == "hash5");
    }

    [Fact]
    public async Task CleanupUserHistoryAsync_MultipleUsers_IsolatesCorrectly()
    {
        InitializeInMemoryDatabase();

        _context.UserPasswordHistories.AddRange(
            new UserPasswordHistory { UserId = 1, PasswordHash = "u1_hash1", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "u1_hash2", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "u1_hash3", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1) },
            new UserPasswordHistory { UserId = 2, PasswordHash = "u2_hash1", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2) },
            new UserPasswordHistory { UserId = 2, PasswordHash = "u2_hash2", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1) }
        );
        await _context.SaveChangesAsync();

        var result = await _service.CleanupUserHistoryAsync(userId: 1, keepCount: 2);

        Assert.Equal(1, result);
        Assert.Equal(2, await _context.UserPasswordHistories.CountAsync(h => h.UserId == 1));
        Assert.Equal(2, await _context.UserPasswordHistories.CountAsync(h => h.UserId == 2));
    }

    [Fact]
    public async Task CleanupAllUsersHistoryAsync_HistoryCountZero_ReturnsZero()
    {
        InitializeInMemoryDatabase();

        _mockPolicyProvider.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordPolicyOptions { HistoryCount = 0 });

        var result = await _service.CleanupAllUsersHistoryAsync();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CleanupAllUsersHistoryAsync_CleansAllUsers()
    {
        InitializeInMemoryDatabase();

        _context.UserPasswordHistories.AddRange(
            new UserPasswordHistory { UserId = 1, PasswordHash = "u1_hash1", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-5) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "u1_hash2", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-4) },
            new UserPasswordHistory { UserId = 1, PasswordHash = "u1_hash3", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3) },
            new UserPasswordHistory { UserId = 2, PasswordHash = "u2_hash1", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-4) },
            new UserPasswordHistory { UserId = 2, PasswordHash = "u2_hash2", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3) },
            new UserPasswordHistory { UserId = 2, PasswordHash = "u2_hash3", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2) },
            new UserPasswordHistory { UserId = 2, PasswordHash = "u2_hash4", HashAlgorithm = "Argon2id", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1) }
        );
        await _context.SaveChangesAsync();

        _mockPolicyProvider.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordPolicyOptions { HistoryCount = 2 });

        var result = await _service.CleanupAllUsersHistoryAsync();

        Assert.Equal(3, result);
        Assert.Equal(2, await _context.UserPasswordHistories.CountAsync(h => h.UserId == 1));
        Assert.Equal(2, await _context.UserPasswordHistories.CountAsync(h => h.UserId == 2));
    }

    [Fact]
    public async Task CleanupAllUsersHistoryAsync_NoHistoryRecords_ReturnsZero()
    {
        InitializeInMemoryDatabase();

        _mockPolicyProvider.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordPolicyOptions { HistoryCount = 5 });

        var result = await _service.CleanupAllUsersHistoryAsync();

        Assert.Equal(0, result);
    }
}
