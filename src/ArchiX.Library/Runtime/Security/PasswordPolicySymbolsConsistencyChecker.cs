namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// allowedSymbols alanýnýn UI örneði ile parametre deðeri arasýndaki tutarlýlýðý kontrol eder (PK-04).
/// </summary>
public static class PasswordPolicySymbolsConsistencyChecker
{
    /// <summary>
    /// Varsayýlan sembol örneði (UI'da gösterilen).
    /// </summary>
    public const string DefaultAllowedSymbols = "!@#$%^&*_-+=:?.,;";

    /// <summary>
    /// Parametre JSON'ýndaki allowedSymbols ile varsayýlan örneði karþýlaþtýrýr.
    /// </summary>
    /// <param name="actualSymbols">JSON'dan okunan allowedSymbols deðeri.</param>
    /// <returns>Tutarsýzlýk varsa açýklama, yoksa null.</returns>
    public static string? CheckConsistency(string actualSymbols)
    {
        if (string.IsNullOrEmpty(actualSymbols))
            return "allowedSymbols boþ, varsayýlan deðer kullanýlmalý.";

        // Karakter kümesini normalize et (sýralama ve tekrar eden karakterleri kaldýr)
        var normalizedActual = NormalizeSymbols(actualSymbols);
        var normalizedDefault = NormalizeSymbols(DefaultAllowedSymbols);

        if (normalizedActual != normalizedDefault)
        {
            return $"allowedSymbols UI örneðinden farklý. Beklenen: '{DefaultAllowedSymbols}', Gerçek: '{actualSymbols}'";
        }

        return null; // Tutarlý
    }

    /// <summary>
    /// Sembol string'ini normalize eder: sýralar, tekrar edenleri kaldýrýr.
    /// </summary>
    private static string NormalizeSymbols(string symbols)
    {
        if (string.IsNullOrEmpty(symbols))
            return string.Empty;

        // Benzersiz karakterleri sýralý olarak al
        var uniqueSorted = symbols
            .Distinct()
            .OrderBy(c => c)
            .ToArray();

        return new string(uniqueSorted);
    }

    /// <summary>
    /// Tutarsýzlýk varsa true döner.
    /// </summary>
    public static bool HasInconsistency(string actualSymbols)
    {
        return CheckConsistency(actualSymbols) != null;
    }
}
