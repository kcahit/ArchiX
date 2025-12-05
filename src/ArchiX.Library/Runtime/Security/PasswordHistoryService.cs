using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Kullanýcýnýn parola geçmiþini yöneten servis (RL-02).
/// </summary>
public sealed class PasswordHistoryService : IPasswordHistoryService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<PasswordHistoryService> _logger;

    public PasswordHistoryService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<PasswordHistoryService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<bool> IsPasswordInHistoryAsync(
        int userId,
        string newPasswordHash,
        int historyCount,
        CancellationToken ct = default)
    {
        if (historyCount <= 0)
            return false; // History kontrolü devre dýþý

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var recentHashes = await db.UserPasswordHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Take(historyCount)
            .Select(h => h.PasswordHash)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        // Argon2 hash'leri doðrudan karþýlaþtýrýlamaz (salt dahil), 
        // bu yüzden string equality kullanýyoruz (ayný hash = ayný parola)
        var found = recentHashes.Contains(newPasswordHash, StringComparer.Ordinal);

        if (found)
        {
            _logger.LogWarning("UserId={UserId} için parola geçmiþte kullanýlmýþ.", userId);
        }

        return found;
    }

    public async Task AddToHistoryAsync(
        int userId,
        string passwordHash,
        string algorithm,
        int historyCount,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        // Önce eski kayýtlarý temizle (yeni kayýt eklenmeden önce)
        if (historyCount > 0)
        {
            var oldRecords = await db.UserPasswordHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAtUtc)
                .Skip(historyCount - 1) // Yeni kayýt için 1 yer aç
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (oldRecords.Count > 0)
            {
                db.UserPasswordHistories.RemoveRange(oldRecords);
                _logger.LogInformation("UserId={UserId} için {Count} eski parola kaydý silindi.", userId, oldRecords.Count);
            }
        }

        // Yeni kayýt ekle
        var newRecord = new UserPasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            HashAlgorithm = algorithm,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            StatusId = 3, // Approved
            CreatedBy = userId
        };

        db.UserPasswordHistories.Add(newRecord);

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        _logger.LogInformation("UserId={UserId} için yeni parola history kaydý eklendi.", userId);
    }
}
