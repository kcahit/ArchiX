namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Kullanýcýnýn parola geçmiþini yöneten servis (RL-02).
/// </summary>
public interface IPasswordHistoryService
{
    /// <summary>
    /// Yeni parolanýn geçmiþte kullanýlýp kullanýlmadýðýný kontrol eder.
    /// </summary>
    /// <param name="userId">Kullanýcý ID.</param>
    /// <param name="newPasswordHash">Yeni parolanýn hash'i.</param>
    /// <param name="historyCount">Kontrol edilecek geçmiþ sayýsý.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>True = geçmiþte kullanýlmýþ, False = yeni.</returns>
    Task<bool> IsPasswordInHistoryAsync(int userId, string newPasswordHash, int historyCount, CancellationToken ct = default);

    /// <summary>
    /// Kullanýcýnýn yeni parola hash'ini geçmiþe ekler ve eski kayýtlarý temizler.
    /// </summary>
    /// <param name="userId">Kullanýcý ID.</param>
    /// <param name="passwordHash">Parola hash'i.</param>
    /// <param name="algorithm">Hash algoritmasý (örn: Argon2id).</param>
    /// <param name="historyCount">Tutulacak maksimum kayýt sayýsý.</param>
    /// <param name="ct">CancellationToken.</param>
    Task AddToHistoryAsync(int userId, string passwordHash, string algorithm, int historyCount, CancellationToken ct = default);
}
