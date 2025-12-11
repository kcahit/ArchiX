using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Tam parola doğrulama servisi (policy + pwned + history + blacklist).
/// </summary>
public sealed class PasswordValidationService
{
    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly IPasswordPwnedChecker _pwnedChecker;
    private readonly IPasswordHistoryService _historyService;
    private readonly IPasswordBlacklistService _blacklistService;
    private readonly IPasswordHasher _hasher;

    public PasswordValidationService(
        IPasswordPolicyProvider policyProvider,
        IPasswordPwnedChecker pwnedChecker,
        IPasswordHistoryService historyService,
        IPasswordBlacklistService blacklistService,
        IPasswordHasher hasher)
    {
        _policyProvider = policyProvider;
        _pwnedChecker = pwnedChecker;
        _historyService = historyService;
        _blacklistService = blacklistService;
        _hasher = hasher;
    }

    public async Task<PasswordValidationResult> ValidateAsync(
        string password,
        int userId,
        int applicationId = 1,
        CancellationToken ct = default)
    {
        var policy = await _policyProvider.GetAsync(applicationId, ct);
        var errors = new List<string>();

        // 1. Policy kuralları (senkron)
        var policyErrors = ValidatePolicy(password, policy);
        errors.AddRange(policyErrors);

        // Policy hatası varsa, diğer kontrolleri atla
        if (errors.Count > 0)
            return new PasswordValidationResult(false, errors);

        // 2. Blacklist kontrolü (async)
        var isBlocked = await _blacklistService.IsWordBlockedAsync(password, applicationId, ct);
        if (isBlocked)
            errors.Add("BLACKLIST");

        // 3. Pwned kontrolü (async - her zaman çalışır)
        var isPwned = await _pwnedChecker.IsPwnedAsync(password, ct);
        if (isPwned)
            errors.Add("PWNED");

        // 4. History kontrolü (async)
        if (policy.HistoryCount > 0)
        {
            var hash = await _hasher.HashAsync(password, policy, ct);
            var inHistory = await _historyService.IsPasswordInHistoryAsync(
                userId, hash, policy.HistoryCount, ct);

            if (inHistory)
                errors.Add("HISTORY");
        }

        return new PasswordValidationResult(errors.Count == 0, errors);
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
/// Parola doğrulama sonucu.
/// </summary>
/// <param name="IsValid">Doğrulama başarılı mı?</param>
/// <param name="Errors">Hata kodları listesi (EMPTY, MIN_LENGTH, BLACKLIST, PWNED, HISTORY vb.).</param>
public sealed record PasswordValidationResult(bool IsValid, IReadOnlyList<string> Errors);
