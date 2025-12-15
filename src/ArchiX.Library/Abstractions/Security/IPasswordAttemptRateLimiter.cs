namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Parola deðiþim/deneme hýz sýnýrlama servisi.
/// Sliding window algoritmasý ile belirli sürede maksimum deneme sayýsýný kontrol eder.
/// </summary>
public interface IPasswordAttemptRateLimiter
{
    /// <summary>
    /// Belirtilen kullanýcý/IP için rate limit aþýlmýþ mý kontrol eder.
    /// </summary>
    /// <param name="key">Kullanýcý ID veya IP adresi (örn: "user:123" veya "ip:192.168.1.1")</param>
    /// <param name="cancellationToken">Ýptal token'ý</param>
    /// <returns>
    /// True: Rate limit aþýldý (429 Too Many Requests döndürülmeli)
    /// False: Devam edilebilir
    /// </returns>
    Task<bool> IsRateLimitExceededAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir parola denemesi kaydeder (baþarýlý veya baþarýsýz).
    /// </summary>
    /// <param name="key">Kullanýcý ID veya IP adresi</param>
    /// <param name="cancellationToken">Ýptal token'ý</param>
    Task RecordAttemptAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kullanýcý/IP için rate limit sayaçlarýný sýfýrlar.
    /// Baþarýlý giriþ sonrasý çaðrýlabilir.
    /// </summary>
    /// <param name="key">Kullanýcý ID veya IP adresi</param>
    /// <param name="cancellationToken">Ýptal token'ý</param>
    Task ResetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kalan süre ve deneme bilgisini döndürür (UI için).
    /// </summary>
    /// <param name="key">Kullanýcý ID veya IP adresi</param>
    /// <param name="cancellationToken">Ýptal token'ý</param>
    /// <returns>
    /// (RemainingAttempts: Kalan deneme hakký, RetryAfterSeconds: Tekrar denemeden önce beklenecek süre)
    /// </returns>
    Task<(int RemainingAttempts, int RetryAfterSeconds)> GetStatusAsync(string key, CancellationToken cancellationToken = default);
}