using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Entities;

using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Tam parola doğrulama servisi (policy + expiration + pwned + history).
/// </summary>
public sealed class PasswordValidationService
{
    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly IPasswordExpirationService _expirationService;
    private readonly IPasswordPwnedChecker _pwnedChecker;
    private readonly IPasswordHistoryService _historyService;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<PasswordValidationService> _logger;

    public PasswordValidationService(
        IPasswordPolicyProvider policyProvider,
        IPasswordExpirationService expirationService,
        IPasswordPwnedChecker pwnedChecker,
        IPasswordHistoryService historyService,
        IPasswordHasher hasher,
        ILogger<PasswordValidationService> logger)
    {
        _policyProvider = policyProvider;
        _expirationService = expirationService;
        _pwnedChecker = pwnedChecker;
        _historyService = historyService;
        _hasher = hasher;
        _logger = logger;
    }

    /// <summary>
    /// Parolayı tüm kurallara göre doğrular (policy + expiration + pwned + history).
    /// </summary>
    /// <param name="password">Düz metin parola.</param>
    /// <param name="user">Kullanıcı entity (expiration kontrolü için gerekli).</param>
    /// <param name="applicationId">Uygulama ID (varsayılan: 1).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Doğrulama sonucu (başarılı/hatalı + hata kodları).</returns>
    public async Task<PasswordValidationResult> ValidateAsync(
        string password,
        User user,
        int applicationId = 1,
        CancellationToken ct = default)
    {
        var errors = new List<string>();

        // 1. Policy kuralları (senkron)
        var policy = await _policyProvider.GetAsync(applicationId, ct).ConfigureAwait(false);
        var policyErrors = PasswordPolicyValidator.Validate(password, policy);
        errors.AddRange(policyErrors);

        // Temel kontroller başarısızsa devam etme (performans optimizasyonu)
        if (errors.Count > 0)
        {
            _logger.LogWarning("Parola policy kurallarını karşılamıyor (UserId: {UserId}, Errors: {Errors})",
                user.Id, string.Join(", ", errors));
            return new PasswordValidationResult(false, errors);
        }

        // 2. Parola yaşlandırma kontrolü (senkron)
        if (_expirationService.IsExpired(user, policy))
        {
            _logger.LogWarning("Parola süresi dolmuş (UserId: {UserId})", user.Id);
            errors.Add("EXPIRED");
        }

        // 3. Pwned kontrolü (async)
        if (await _pwnedChecker.IsPwnedAsync(password, ct).ConfigureAwait(false))
        {
            _logger.LogWarning("Parola HIBP veritabanında bulundu (UserId: {UserId})", user.Id);
            errors.Add("PWNED");
        }

        // 4. History kontrolü (async)
        if (policy.HistoryCount > 0)
        {
            // ✅ Parolayı hash'le (history karşılaştırması için)
            var passwordHash = await _hasher.HashAsync(password, policy, ct).ConfigureAwait(false);

            var inHistory = await _historyService.IsPasswordInHistoryAsync(
                user.Id,
                passwordHash,
                policy.HistoryCount,
                ct).ConfigureAwait(false);

            if (inHistory)
            {
                _logger.LogWarning("Parola geçmişte kullanılmış (UserId: {UserId})", user.Id);
                errors.Add("HISTORY");
            }
        }

        var isValid = errors.Count == 0;
        if (isValid)
        {
            _logger.LogInformation("Parola doğrulaması başarılı (UserId: {UserId})", user.Id);
        }

        return new PasswordValidationResult(isValid, errors);
    }
}

/// <summary>
/// Parola doğrulama sonucu.
/// </summary>
/// <param name="IsValid">Doğrulama başarılı mı?</param>
/// <param name="Errors">Hata kodları listesi (EMPTY, MIN_LENGTH, EXPIRED, PWNED, HISTORY vb.).</param>
public sealed record PasswordValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);
