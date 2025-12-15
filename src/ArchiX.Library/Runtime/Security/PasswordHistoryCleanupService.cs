using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Service for cleaning up old password history records.
/// </summary>
public class PasswordHistoryCleanupService : IPasswordHistoryCleanupService
{
    private readonly AppDbContext _context;
    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly ILogger<PasswordHistoryCleanupService> _logger;

    public PasswordHistoryCleanupService(
        AppDbContext context,
        IPasswordPolicyProvider policyProvider,
        ILogger<PasswordHistoryCleanupService> logger)
    {
        _context = context;
        _policyProvider = policyProvider;
        _logger = logger;
    }

    public async Task<int> CleanupUserHistoryAsync(int userId, int keepCount, CancellationToken cancellationToken = default)
    {
        if (keepCount <= 0)
        {
            return 0;
        }

        var toDelete = await _context.UserPasswordHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip(keepCount)
            .ToListAsync(cancellationToken);

        if (toDelete.Count == 0)
        {
            return 0;
        }

        _context.UserPasswordHistories.RemoveRange(toDelete);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cleaned up {Count} password history records for user {UserId}",
            toDelete.Count,
            userId);

        return toDelete.Count;
    }

    public async Task<int> CleanupAllUsersHistoryAsync(CancellationToken cancellationToken = default)
    {
        var policy = await _policyProvider.GetAsync(applicationId: 1, cancellationToken);

        if (policy.HistoryCount <= 0)
        {
            _logger.LogWarning("HistoryCount is 0, skipping cleanup");
            return 0;
        }

        var userIds = await _context.UserPasswordHistories
            .Select(h => h.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalDeleted = 0;

        foreach (var userId in userIds)
        {
            var deleted = await CleanupUserHistoryAsync(userId, policy.HistoryCount, cancellationToken);
            totalDeleted += deleted;
        }

        _logger.LogInformation(
            "Cleaned up {Count} password history records across {UserCount} users",
            totalDeleted,
            userIds.Count);

        return totalDeleted;
    }
}
