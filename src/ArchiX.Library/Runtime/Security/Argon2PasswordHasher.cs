using System.Security.Cryptography;
using System.Text;

using ArchiX.Library.Abstractions.Security;

using Isopoh.Cryptography.Argon2;

namespace ArchiX.Library.Runtime.Security;

internal sealed class Argon2PasswordHasher : IPasswordHasher
{
    // 3) Pepper yönetimi: ENV → ARCHIX_PEPPER; yoksa fallback sabit değer.
    private static string GetPepper()
        => Environment.GetEnvironmentVariable("ARCHIX_PEPPER") ?? "PEPPER_PLACEHOLDER";

    public Task<string> HashAsync(string password, PasswordPolicyOptions policy, CancellationToken ct = default)
    {
        // 1) İptal kontrolü
        ct.ThrowIfCancellationRequested();

        // 2) Null kontrolleri
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(policy.Hash);

        var h = policy.Hash;
        var pepper = h.PepperEnabled ? GetPepper() : string.Empty;

        // Salt üretimi
        var salt = RandomNumberGenerator.GetBytes(h.SaltLength);

        var config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing, // Argon2id
            TimeCost = h.Iterations,
            MemoryCost = h.MemoryKb,
            Lanes = h.Parallelism,
            Threads = h.Parallelism,
            Salt = salt,
            HashLength = h.HashLength,
            Password = Encoding.UTF8.GetBytes(password + pepper)
        };

        // 7) Kaynak temizliği (using)
        var argon2 = new Argon2(config);
        using var secureHash = argon2.Hash(); // SecureArray<byte>

        // 4) Yeni format: yalnızca standart Argon2 encoded string (legacy sonek yok)
        var encoded = EncodeExtension.EncodeString(config, secureHash.Buffer);
        return Task.FromResult(encoded);
    }

    public Task<bool> VerifyAsync(string password, string encodedHash, PasswordPolicyOptions policy, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(encodedHash);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(policy.Hash);

        try
        {
            // 4) Sadelik: Yeni kayıtlar legacy sonek içermez.
            // 6) Geriye dönük uyumluluk: varsa "|pep=" soneğini ayıkla (geçiş sürecinde eski kayıtlar için).
            string baseEncoded = encodedHash;
            bool legacyPepEnabled = false;

            var suffixIndex = encodedHash.LastIndexOf("|pep=", StringComparison.OrdinalIgnoreCase);
            if (suffixIndex >= 0)
            {
                var flagSpan = encodedHash.AsSpan(suffixIndex + 5); // "pep=" sonrası
                if (bool.TryParse(flagSpan, out var legacyFlag))
                    legacyPepEnabled = legacyFlag;
                baseEncoded = encodedHash[..suffixIndex];
            }

            var effectivePepperEnabled = legacyPepEnabled || policy.Hash.PepperEnabled;
            var pepper = effectivePepperEnabled ? GetPepper() : string.Empty;

            var ok = Argon2.Verify(baseEncoded, password + pepper);
            return Task.FromResult(ok);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
