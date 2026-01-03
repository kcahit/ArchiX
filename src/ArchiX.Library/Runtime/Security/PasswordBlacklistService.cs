using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Parola blacklist yönetimi ve kontrol servisi.
/// </summary>
public sealed class PasswordBlacklistService : IPasswordBlacklistService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
    private const int MaxWordLength = 256;
    private const string CacheKeyPrefix = "password_blacklist_";

    public PasswordBlacklistService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<bool> IsWordBlockedAsync(string password, int applicationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var blockedWords = await GetOrLoadWordsAsync(applicationId, ct).ConfigureAwait(false);
        foreach (var word in blockedWords)
        {
            if (password.Contains(word, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public async Task<IReadOnlyList<string>> GetBlockedWordsAsync(int applicationId, CancellationToken ct = default)
    {
        return await GetOrLoadWordsAsync(applicationId, ct).ConfigureAwait(false);
    }

    public async Task<bool> AddWordAsync(string word, int applicationId, CancellationToken ct = default)
    {
        var normalized = Normalize(word);
        if (normalized is null)
            return false;

        var exists = await _context.PasswordBlacklists
            .IgnoreQueryFilters()
            .AnyAsync(x => x.ApplicationId == applicationId && x.Word == normalized, ct)
            .ConfigureAwait(false);

        if (exists)
            return false;

        var entity = new PasswordBlacklist
        {
            ApplicationId = applicationId,
            Word = normalized,
            CreatedBy = 0,
            StatusId = BaseEntity.ApprovedStatusId,
            LastStatusBy = 0
        };

        _context.PasswordBlacklists.Add(entity);
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);
        InvalidateCache(applicationId);
        return true;
    }

    public async Task<bool> RemoveWordAsync(string word, int applicationId, CancellationToken ct = default)
    {
        var normalized = Normalize(word);
        if (normalized is null)
            return false;

        var entity = await _context.PasswordBlacklists
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.Word == normalized, ct)
            .ConfigureAwait(false);

        if (entity is null)
            return false;

        entity.SoftDelete(0);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = 0;

        await _context.SaveChangesAsync(ct).ConfigureAwait(false);
        InvalidateCache(applicationId);
        return true;
    }

    public async Task<int> GetCountAsync(int applicationId, CancellationToken ct = default)
    {
        return await _context.PasswordBlacklists
            .CountAsync(x => x.ApplicationId == applicationId && x.StatusId != BaseEntity.DeletedStatusId, ct)
            .ConfigureAwait(false);
    }

    public void InvalidateCache(int applicationId)
    {
        _cache.Remove(CacheKey(applicationId));
    }

    private async Task<IReadOnlyList<string>> GetOrLoadWordsAsync(int applicationId, CancellationToken ct)
    {
        var cacheKey = CacheKey(applicationId);
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
            return cached;

        var words = await _context.PasswordBlacklists
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId && x.StatusId != BaseEntity.DeletedStatusId)
            .OrderBy(x => x.Word)
            .Select(x => x.Word)
            .ToArrayAsync(ct)
            .ConfigureAwait(false);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        };
        _cache.Set(cacheKey, words, options);
        return words;
    }

    private static string? Normalize(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return null;

        var trimmed = word.Trim();
        if (trimmed.Length > MaxWordLength)
            trimmed = trimmed[..MaxWordLength];

        return trimmed.ToLowerInvariant();
    }

    private static string CacheKey(int applicationId) => $"{CacheKeyPrefix}{applicationId}";
}
