using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Tam parola doðrulama servisi (policy + expiration + dynamic blacklist + pwned + history).
/// </summary>
public sealed class PasswordValidationService
{
    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly IPasswordPwnedChecker _pwnedChecker;
    private readonly IPasswordHistoryService _historyService;
    private readonly IPasswordBlacklistService _blacklistService;
    private readonly IPasswordHasher _hasher;
    private readonly IPasswordExpirationService _expirationService;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public PasswordValidationService(
        IPasswordPolicyProvider policyProvider,
        IPasswordPwnedChecker pwnedChecker,
        IPasswordHistoryService historyService,
        IPasswordBlacklistService blacklistService,
        IPasswordHasher hasher,
        IPasswordExpirationService expirationService,
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _policyProvider = policyProvider;
        _pwnedChecker = pwnedChecker;
        _historyService = historyService;
        _blacklistService = blacklistService;
        _hasher = hasher;
        _expirationService = expirationService;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PasswordValidationResult> ValidateAsync(
        string password,
        int userId,
        int applicationId = 1,
        CancellationToken ct = default)
    {
        var policy = await _policyProvider.GetAsync(applicationId, ct).ConfigureAwait(false);
        var errors = new List<string>();

        var policyErrors = ValidatePolicy(password, policy);
        errors.AddRange(policyErrors);
        if (errors.Count > 0)
            return new PasswordValidationResult(false, errors);

        if (RequiresExpirationCheck(policy))
        {
            var user = await LoadUserAsync(userId, ct).ConfigureAwait(false);
            if (user is not null && _expirationService.IsExpired(user, policy))
            {
                errors.Add("EXPIRED");
                return new PasswordValidationResult(false, errors);
            }
        }

        var dynamicBlocked = await _blacklistService.IsWordBlockedAsync(password, applicationId, ct).ConfigureAwait(false);
        if (dynamicBlocked)
        {
            errors.Add("DYNAMIC_BLOCK");
            return new PasswordValidationResult(false, errors);
        }

        var isPwned = await _pwnedChecker.IsPwnedAsync(password, ct).ConfigureAwait(false);
        if (isPwned)
            errors.Add("PWNED");

        if (policy.HistoryCount > 0)
        {
            var hash = await _hasher.HashAsync(password, policy, ct).ConfigureAwait(false);
            var inHistory = await _historyService.IsPasswordInHistoryAsync(userId, hash, policy.HistoryCount, ct).ConfigureAwait(false);
            if (inHistory)
                errors.Add("HISTORY");
        }

        return new PasswordValidationResult(errors.Count == 0, errors);
    }

    private static bool RequiresExpirationCheck(PasswordPolicyOptions policy) => policy.MaxPasswordAgeDays is > 0;

    private async Task<User?> LoadUserAsync(int userId, CancellationToken ct)
    {
        if (userId <= 0)
            return null;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            .ConfigureAwait(false);
    }

    private static List<string> ValidatePolicy(string password, PasswordPolicyOptions policy)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("EMPTY");
            return errors;
        }

        if (password.Length < policy.MinLength)
            errors.Add("MIN_LENGTH");

        if (password.Length > policy.MaxLength)
            errors.Add("MAX_LENGTH");

        if (policy.RequireUpper && !password.Any(char.IsUpper))
            errors.Add("REQ_UPPER");

        if (policy.RequireLower && !password.Any(char.IsLower))
            errors.Add("REQ_LOWER");

        if (policy.RequireDigit && !password.Any(char.IsDigit))
            errors.Add("REQ_DIGIT");

        if (policy.RequireSymbol && !password.Any(c => policy.AllowedSymbols.Contains(c)))
            errors.Add("REQ_SYMBOL");

        if (policy.MinDistinctChars > 0)
        {
            var distinctCount = password.Distinct().Count();
            if (distinctCount < policy.MinDistinctChars)
                errors.Add("MIN_DISTINCT");
        }

        if (policy.MaxRepeatedSequence > 0 && HasRepeatedSequence(password, policy.MaxRepeatedSequence))
            errors.Add("REPEAT_SEQ");

        if (policy.BlockList.Any(blocked => password.Contains(blocked, StringComparison.OrdinalIgnoreCase)))
            errors.Add("BLOCK_LIST");

        return errors;
    }

    private static bool HasRepeatedSequence(string password, int maxRepeated)
    {
        for (int i = 0; i < password.Length - maxRepeated; i++)
        {
            var current = password[i];
            var count = 1;
            for (int j = i + 1; j < password.Length && password[j] == current; j++)
            {
                count++;
                if (count > maxRepeated)
                    return true;
            }
        }
        return false;
    }
}

/// <summary>
/// Parola doðrulama sonucu.
/// </summary>
/// <param name="IsValid">Doðrulama baþarýlý mý?</param>
/// <param name="Errors">Hata kodlarý listesi.</param>
public sealed record PasswordValidationResult(bool IsValid, IReadOnlyList<string> Errors);
