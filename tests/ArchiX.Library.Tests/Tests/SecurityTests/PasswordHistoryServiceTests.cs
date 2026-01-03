using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordHistoryServiceTests
{
    private readonly ILogger<PasswordHistoryService> _logger;

    public PasswordHistoryServiceTests()
    {
        _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<PasswordHistoryService>();
    }

    [Fact]
    public async Task IsPasswordInHistoryAsync_ReturnsFalse_WhenHistoryIsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"history-empty-{Guid.NewGuid()}")
            .Options;
        var dbFactory = new TestDbContextFactory(options);
        var service = new PasswordHistoryService(dbFactory, _logger);

        // Act
        var isInHistory = await service.IsPasswordInHistoryAsync(1, "hash123", 3);

        // Assert
        Assert.False(isInHistory);
    }

    [Fact]
    public async Task IsPasswordInHistoryAsync_ReturnsTrue_WhenPasswordFoundInHistory()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"history-found-{Guid.NewGuid()}")
            .Options;
        var dbFactory = new TestDbContextFactory(options);
        var service = new PasswordHistoryService(dbFactory, _logger);

        await service.AddToHistoryAsync(1, "hash123", "Argon2id", 5);
        await service.AddToHistoryAsync(1, "hash456", "Argon2id", 5);

        // Act
        var isInHistory = await service.IsPasswordInHistoryAsync(1, "hash123", 5);

        // Assert
        Assert.True(isInHistory);
    }

    [Fact]
    public async Task IsPasswordInHistoryAsync_ReturnsFalse_WhenPasswordNotInHistory()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"history-notfound-{Guid.NewGuid()}")
            .Options;
        var dbFactory = new TestDbContextFactory(options);
        var service = new PasswordHistoryService(dbFactory, _logger);

        await service.AddToHistoryAsync(1, "hash123", "Argon2id", 5);

        // Act
        var isInHistory = await service.IsPasswordInHistoryAsync(1, "hash999", 5);

        // Assert
        Assert.False(isInHistory);
    }

    [Fact]
    public async Task AddToHistoryAsync_RemovesOldRecords_WhenLimitExceeded()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"history-limit-{Guid.NewGuid()}")
            .Options;
        var dbFactory = new TestDbContextFactory(options);
        var service = new PasswordHistoryService(dbFactory, _logger);

        // Act - 3 kayýt ekle ama limit 2
        await service.AddToHistoryAsync(1, "hash1", "Argon2id", 2);
        await service.AddToHistoryAsync(1, "hash2", "Argon2id", 2);
        await service.AddToHistoryAsync(1, "hash3", "Argon2id", 2);

        // Assert
        await using var db = await dbFactory.CreateDbContextAsync();
        var count = await db.UserPasswordHistories.CountAsync(h => h.UserId == 1);
        Assert.Equal(2, count);

        // En eski (hash1) silinmiþ olmalý
        var isHash1Present = await service.IsPasswordInHistoryAsync(1, "hash1", 2);
        Assert.False(isHash1Present);

        // Yeni olanlar olmalý
        var isHash3Present = await service.IsPasswordInHistoryAsync(1, "hash3", 2);
        Assert.True(isHash3Present);
    }

    [Fact]
    public async Task IsPasswordInHistoryAsync_ReturnsFalse_WhenHistoryCountIsZero()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"history-disabled-{Guid.NewGuid()}")
            .Options;
        var dbFactory = new TestDbContextFactory(options);
        var service = new PasswordHistoryService(dbFactory, _logger);

        await service.AddToHistoryAsync(1, "hash123", "Argon2id", 5);

        // Act - historyCount=0 (devre dýþý)
        var isInHistory = await service.IsPasswordInHistoryAsync(1, "hash123", 0);

        // Assert
        Assert.False(isInHistory);
    }

    // Helper class for IDbContextFactory
    private class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext()
        {
            return new AppDbContext(_options);
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AppDbContext(_options));
        }
    }
}
