namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Have I Been Pwned (HIBP) API ile parola sýzýntý kontrolü (RL-01).
/// k-Anonymity modeli: Sadece SHA-1 hash'inin ilk 5 karakteri gönderilir.
/// </summary>
public interface IPasswordPwnedChecker
{
    /// <summary>
    /// Parolanýn sýzdýrýlmýþ olup olmadýðýný kontrol eder.
    /// </summary>
    /// <param name="password">Kontrol edilecek parola (düz metin).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>True = sýzdýrýlmýþ, False = güvenli.</returns>
    Task<bool> IsPwnedAsync(string password, CancellationToken ct = default);

    /// <summary>
    /// Parolanýn kaç kez sýzdýrýldýðýný döner.
    /// </summary>
    /// <param name="password">Kontrol edilecek parola.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Sýzýntý sayýsý (0 = güvenli).</returns>
    Task<int> GetPwnedCountAsync(string password, CancellationToken ct = default);
}