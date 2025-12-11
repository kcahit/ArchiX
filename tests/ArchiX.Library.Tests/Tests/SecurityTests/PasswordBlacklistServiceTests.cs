using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public sealed class PasswordBlacklistServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly PasswordBlacklistService _service;

    public PasswordBlacklistServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"BlacklistTest_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new PasswordBlacklistService(_context, _cache);

        // Seed default blacklist
        _context.PasswordBlacklists.AddRange(
            new PasswordBlacklist { Id = 1, ApplicationId = 1, Word = "password", StatusId = 3, CreatedBy = 0 },
            new PasswordBlacklist { Id = 2, ApplicationId = 1, Word = "123456", StatusId = 3, CreatedBy = 0 },
            new PasswordBlacklist { Id = 3, ApplicationId = 2, Word = "admin", StatusId = 3, CreatedBy = 0 }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task IsWordBlockedAsync_BlockedWord_ReturnsTrue()
    {
        var result = await _service.IsWordBlockedAsync("password", applicationId: 1);
        Assert.True(result);
    }

    [Fact]
    public async Task IsWordBlockedAsync_CaseInsensitive_ReturnsTrue()
    {
        var result = await _service.IsWordBlockedAsync("PASSWORD", applicationId: 1);
        Assert.True(result);
    }

    [Fact]
    public async Task IsWordBlockedAsync_NotBlocked_ReturnsFalse()
    {
        var result = await _service.IsWordBlockedAsync("validword", applicationId: 1);
        Assert.False(result);
    }

    [Fact]
    public async Task IsWordBlockedAsync_DifferentApp_ReturnsFalse()
    {
        var result = await _service.IsWordBlockedAsync("admin", applicationId: 1);
        Assert.False(result); // "admin" sadece App 2'de bloklu
    }

    [Fact]
    public async Task AddWordAsync_NewWord_ReturnsTrue()
    {
        var result = await _service.AddWordAsync("newblocked", applicationId: 1);
        Assert.True(result);

        var isBlocked = await _service.IsWordBlockedAsync("newblocked", applicationId: 1);
        Assert.True(isBlocked);
    }

    [Fact]
    public async Task AddWordAsync_ExistingWord_ReturnsFalse()
    {
        var result = await _service.AddWordAsync("password", applicationId: 1);
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveWordAsync_ExistingWord_ReturnsTrue()
    {
        var result = await _service.RemoveWordAsync("password", applicationId: 1);
        Assert.True(result);

        var isBlocked = await _service.IsWordBlockedAsync("password", applicationId: 1);
        Assert.False(isBlocked); // Soft delete sonrası bloklu olmamalı
    }

    [Fact]
    public async Task GetBlockedWordsAsync_ReturnsAllWords()
    {
        var words = await _service.GetBlockedWordsAsync(applicationId: 1);

        Assert.Equal(2, words.Count);
        Assert.Contains("password", words);
        Assert.Contains("123456", words);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        var count = await _service.GetCountAsync(applicationId: 1);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task InvalidateCache_ClearsCachedData()
    {
        // İlk çağrı cache'e yükler
        await _service.IsWordBlockedAsync("password", applicationId: 1);

        // Direkt DB'ye ekle (cache'i atlamak için)
        _context.PasswordBlacklists.Add(new PasswordBlacklist
        {
            ApplicationId = 1,
            Word = "newword",
            StatusId = 3,
            CreatedBy = 0
        });
        await _context.SaveChangesAsync();

        // ✅ Cache'de eski veri var, yeni kelime YOK
        var beforeInvalidate = await _service.IsWordBlockedAsync("newword", applicationId: 1);
        Assert.False(beforeInvalidate); // Cache eski, newword yok

        _service.InvalidateCache(applicationId: 1);

        // ✅ Cache temizlendi, DB'den tekrar okumalı, şimdi VAR
        var afterInvalidate = await _service.IsWordBlockedAsync("newword", applicationId: 1);
        Assert.True(afterInvalidate);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _cache.Dispose();
    }
}
