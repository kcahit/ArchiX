namespace ArchiX.Library.Abstractions.Security;

public interface IPasswordPolicyAdminService
{
    Task<string> GetRawJsonAsync(int applicationId = 1, CancellationToken ct = default);
    Task UpdateAsync(string json, int applicationId = 1, CancellationToken ct = default);
    Task<SecurityDashboardData> GetDashboardDataAsync(int applicationId, CancellationToken ct = default);
    Task<IReadOnlyList<PasswordBlacklistWordDto>> GetBlacklistAsync(int applicationId, CancellationToken ct = default);
    Task<bool> TryAddBlacklistWordAsync(int applicationId, string word, int createdByUserId, CancellationToken ct = default);
    Task<bool> TryRemoveBlacklistWordAsync(int wordId, int removedByUserId, CancellationToken ct = default);
    Task<IReadOnlyList<PasswordPolicyAuditDto>> GetAuditTrailAsync(int applicationId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task<AuditDiffDto?> GetAuditDiffAsync(int auditId, CancellationToken ct = default);
    Task<IReadOnlyList<UserPasswordHistoryEntryDto>> GetUserPasswordHistoryAsync(int userId, int take, CancellationToken ct = default);
    Task<PolicyTestResultDto> ValidatePasswordAsync(string password, int userId, int applicationId, CancellationToken ct = default);
}

public sealed record SecurityDashboardData(
    PasswordPolicyOptions Policy,
    int BlacklistWordCount,
    int ExpiredPasswordCount,
    IReadOnlyDictionary<string, int> Last30DaysErrors,
    IReadOnlyList<RecentAuditSummary> RecentChanges);

public sealed record RecentAuditSummary(
    int AuditId,
    DateTimeOffset ChangedAt,
    string UserDisplayName,
    string Summary);

public sealed record PasswordBlacklistWordDto(
    int Id,
    string Word,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc,
    bool IsActive);

public sealed record PasswordPolicyAuditDto(
    int AuditId,
    DateTimeOffset ChangedAt,
    string UserDisplayName,
    string ActionSummary);

public sealed record AuditDiffDto(
    int AuditId,
    string OldJson,
    string NewJson);

public sealed record UserPasswordHistoryEntryDto(
    int UserId,
    string UserName,
    string MaskedHash,
    string HashAlgorithm,
    DateTimeOffset CreatedAtUtc,
    bool IsExpired);

public sealed record PolicyTestResultDto(
    bool IsValid,
    IReadOnlyList<string> Errors,
    int StrengthScore,
    int? HistoryCheckResult,
    int? PwnedCount);
