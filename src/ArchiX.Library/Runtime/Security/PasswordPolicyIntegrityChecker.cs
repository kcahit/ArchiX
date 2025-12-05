using System.Security.Cryptography;
using System.Text;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// PasswordPolicy JSON'u için HMAC tabanlý bütünlük doðrulamasý (PK-10, opsiyonel).
/// </summary>
public static class PasswordPolicyIntegrityChecker
{
    private const string SecretKeyEnvVar = "ARCHIX_POLICY_HMAC_KEY";

    /// <summary>
    /// JSON için HMAC-SHA256 imzasý üretir.
    /// </summary>
    public static string ComputeSignature(string json)
    {
        var key = GetSecretKey();
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException($"HMAC anahtarý tanýmlý deðil: {SecretKeyEnvVar}");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// JSON ve imzayý doðrular.
    /// </summary>
    public static bool VerifySignature(string json, string signature)
    {
        var key = GetSecretKey();
        if (string.IsNullOrEmpty(key))
            return false; // Anahtar yoksa doðrulama yapýlamaz

        var expected = ComputeSignature(json);
        
        // Sabit zamanlý karþýlaþtýrma (timing attack'lere karþý)
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(expected),
            Convert.FromBase64String(signature));
    }

    /// <summary>
    /// HMAC anahtarýný ortam deðiþkeninden okur.
    /// </summary>
    private static string? GetSecretKey()
    {
        return Environment.GetEnvironmentVariable(SecretKeyEnvVar);
    }

    /// <summary>
    /// HMAC özelliði etkin mi kontrol eder.
    /// </summary>
    public static bool IsEnabled()
    {
        return !string.IsNullOrEmpty(GetSecretKey());
    }
}
