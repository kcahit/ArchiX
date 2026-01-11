namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Parola entropi (karmaþýklýk) hesaplama servisi.
/// Shannon Entropy algoritmasý kullanarak parola gücünü ölçer.
/// </summary>
public interface IPasswordEntropyCalculator
{
    /// <summary>
    /// Parolanýn Shannon Entropy deðerini hesaplar (bits/char).
    /// </summary>
    /// <param name="password">Hesaplanacak parola</param>
    /// <returns>Entropy deðeri (bits/char). Yüksek deðer = güçlü parola</returns>
    double CalculateEntropy(string password);

    /// <summary>
    /// Parolanýn toplam entropy deðerini hesaplar (bits).
    /// </summary>
    /// <param name="password">Hesaplanacak parola</param>
    /// <returns>Toplam entropy (bits)</returns>
    double CalculateTotalEntropy(string password);

    /// <summary>
    /// Parolanýn minimum entropy gereksinimini karþýlayýp karþýlamadýðýný kontrol eder.
    /// </summary>
    /// <param name="password">Kontrol edilecek parola</param>
    /// <param name="minEntropyBits">Minimum gerekli entropy (bits)</param>
    /// <returns>true = yeterli entropy, false = yetersiz</returns>
    bool MeetsMinimumEntropy(string password, double minEntropyBits);
}
