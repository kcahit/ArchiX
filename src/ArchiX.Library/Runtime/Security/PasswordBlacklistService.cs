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

    public PasswordBlacklistService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<bool> IsWordBlockedAsync(string word, int applicationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(word))
            return false;

        var cacheKey = $"blacklist_{applicationId}";
        var blockedWords = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _context.PasswordBlacklists
                .Where(x => x.ApplicationId == applicationId)
                .Select(x => x.Word.ToLowerInvariant())
                .ToHashSetAsync(ct);
        });

        return blockedWords?.Contains(word.ToLowerInvariant()) ?? false;
    }

    public async Task<IReadOnlyList<string>> GetBlockedWordsAsync(int applicationId, CancellationToken ct = default)
    {
        return await _context.PasswordBlacklists
            .Where(x => x.ApplicationId == applicationId)
            .OrderBy(x => x.Word)
            .Select(x => x.Word)
            .ToListAsync(ct);
    }

    public async Task<bool> AddWordAsync(string word, int applicationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(word))
            return false;

        var exists = await _context.PasswordBlacklists
            .AnyAsync(x => x.ApplicationId == applicationId && x.Word == word, ct);

        if (exists)
            return false;

        var entity = new PasswordBlacklist
        {
            ApplicationId = applicationId,
            Word = word,
            CreatedBy = 0,
            StatusId = BaseEntity.ApprovedStatusId
        };

        _context.PasswordBlacklists.Add(entity);
        await _context.SaveChangesAsync(ct);

        InvalidateCache(applicationId);
        return true;
    }

    public async Task<bool> RemoveWordAsync(string word, int applicationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(word))
            return false;

        var entity = await _context.PasswordBlacklists
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.Word == word, ct);

        if (entity == null)
            return false;

        entity.SoftDelete(0);
        await _context.SaveChangesAsync(ct);

        InvalidateCache(applicationId);
        return true;
    }

    public async Task<int> GetCountAsync(int applicationId, CancellationToken ct = default)
    {
        return await _context.PasswordBlacklists
            .CountAsync(x => x.ApplicationId == applicationId, ct);
    }

    public void InvalidateCache(int applicationId)
    {
        var cacheKey = $"blacklist_{applicationId}";
        _cache.Remove(cacheKey);
    }
}