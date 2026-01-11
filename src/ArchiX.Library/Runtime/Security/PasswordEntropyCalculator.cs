namespace ArchiX.Library.Runtime.Security;

using ArchiX.Library.Abstractions.Security;

/// <summary>
/// Shannon Entropy algoritması ile parola karmaşıklığı hesaplama implementasyonu.
/// Entropy = -Σ(p(xi) * log2(p(xi))) formülü kullanılır.
/// </summary>
public class PasswordEntropyCalculator : IPasswordEntropyCalculator
{
    /// <inheritdoc/>
    public double CalculateEntropy(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return 0.0;
        }

        // Karakter frekanslarını hesapla
        var frequencies = new Dictionary<char, int>();
        foreach (char c in password)
        {
            frequencies[c] = frequencies.GetValueOrDefault(c, 0) + 1;
        }

        // Shannon Entropy: -Σ(p(xi) * log2(p(xi)))
        double entropy = 0.0;
        int length = password.Length;

        foreach (var freq in frequencies.Values)
        {
            double probability = (double)freq / length;
            entropy -= probability * Math.Log2(probability);
        }

        return entropy;
    }

    /// <inheritdoc/>
    public double CalculateTotalEntropy(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return 0.0;
        }

        double entropyPerChar = CalculateEntropy(password);
        return entropyPerChar * password.Length;
    }

    /// <inheritdoc/>
    public bool MeetsMinimumEntropy(string password, double minEntropyBits)
    {
        if (minEntropyBits <= 0)
        {
            return true; // Entropy kontrolü devre dışı
        }

        double totalEntropy = CalculateTotalEntropy(password);
        return totalEntropy >= minEntropyBits;
    }
}
