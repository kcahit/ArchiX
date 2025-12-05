using ArchiX.Library.Abstractions.Security;

using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Tam parola doğrulama servisi (policy + pwned + history).
/// </summary>
public sealed class PasswordValidationService
{
    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly IPasswordPwnedChecker _pwnedChecker;
    private readonly IPasswordHistoryService _historyService;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<PasswordValidationService> _logger;

    public PasswordValidationService(
        IPasswordPolicyProvider policyProvider,
        IPasswordPwnedChecker pwnedChecker,
        IPasswordHistoryService historyService,
        IPasswordHasher hasher,
        ILogger<PasswordValidationService> logger)
    {
        _policyProvider = policyProvider;
        _pwnedChecker = pwnedChecker;
        _historyService = historyService;
        _hasher = hasher;
        _logger = logger;
    }

    /// <summary>
    /// Parolayı tüm kurallara göre doğrular (policy + pwned + history).
    /// </summary>
    /// <param name="password">Düz metin parola.</param>
    /// <param name="userId">Kullanıcı ID.</param>
    /// <param name="applicationId">Uygulama ID (varsayılan: 1).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Doğrulama sonucu (başarılı/hatalı + hata kodları).</returns>
    public async Task<PasswordValidationResult> ValidateAsync(
        string password,
        int userId,
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
                userId, string.Join(", ", errors));
            return new PasswordValidationResult(false, errors);
        }

        // 2. Pwned kontrolü (async)
        if (await _pwnedChecker.IsPwnedAsync(password, ct).ConfigureAwait(false))
        {
            _logger.LogWarning("Parola HIBP veritabanında bulundu (UserId: {UserId})", userId);
            errors.Add("PWNED");
        }

        // 3. History kontrolü (async)
        if (policy.HistoryCount > 0)
        {
            // ✅ Parolayı hash'le (history karşılaştırması için)
            var passwordHash = await _hasher.HashAsync(password, policy, ct).ConfigureAwait(false);

            var inHistory = await _historyService.IsPasswordInHistoryAsync(
                userId,
                passwordHash, // ✅ HASH gönderiliyor
                policy.HistoryCount,
                ct).ConfigureAwait(false);

            if (inHistory)
            {
                _logger.LogWarning("Parola geçmişte kullanılmış (UserId: {UserId})", userId);
                errors.Add("HISTORY");
            }
        }

        var isValid = errors.Count == 0;
        if (isValid)
        {
            _logger.LogInformation("Parola doğrulaması başarılı (UserId: {UserId})", userId);
        }

        return new PasswordValidationResult(isValid, errors);
    }
}

/// <summary>
/// Parola doğrulama sonucu.
/// </summary>
/// <param name="IsValid">Doğrulama başarılı mı?</param>
/// <param name="Errors">Hata kodları listesi (EMPTY, MIN_LENGTH, PWNED, HISTORY vb.).</param>
public sealed record PasswordValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);
