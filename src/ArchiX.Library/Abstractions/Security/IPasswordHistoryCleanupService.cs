namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Provides functionality to clean up old password history records.
/// </summary>
public interface IPasswordHistoryCleanupService
{
    /// <summary>
    /// Removes old password history records for a specific user, keeping only the most recent entries.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="keepCount">Number of most recent records to keep.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of records deleted.</returns>
    Task<int> CleanupUserHistoryAsync(int userId, int keepCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes old password history records for all users based on policy settings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total number of records deleted.</returns>
    Task<int> CleanupAllUsersHistoryAsync(CancellationToken cancellationToken = default);
}