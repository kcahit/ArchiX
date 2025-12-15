namespace ArchiX.Library.Abstractions.Security;

public sealed record PasswordHashFallbackOptions
{
    public string Algorithm { get; init; } = "PBKDF2-SHA512";
    public int Iterations { get; init; } = 210000;
}

public sealed record PasswordHashOptions
{
    public string Algorithm { get; init; } = "Argon2id";
    public int MemoryKb { get; init; } = 65536;
    public int Parallelism { get; init; } = 2;
    public int Iterations { get; init; } = 3;
    public int SaltLength { get; init; } = 16;
    public int HashLength { get; init; } = 32;
    public bool PepperEnabled { get; init; } = false;
    public PasswordHashFallbackOptions Fallback { get; init; } = new();
}

public sealed record PasswordPolicyOptions
{
    public int Version { get; init; } = 1;
    public int MinLength { get; init; } = 12;
    public int MaxLength { get; init; } = 128;
    public bool RequireUpper { get; init; } = true;
    public bool RequireLower { get; init; } = true;
    public bool RequireDigit { get; init; } = true;
    public bool RequireSymbol { get; init; } = true;
    public string AllowedSymbols { get; init; } = "!@#$%^&*_-+=:?.,;";
    public int MinDistinctChars { get; init; } = 5;
    public int MaxRepeatedSequence { get; init; } = 3;
    public string[] BlockList { get; init; } = new[] { "password", "123456", "qwerty", "admin" };
    public int HistoryCount { get; init; } = 10;
    public int LockoutThreshold { get; init; } = 5;
    public int LockoutSeconds { get; init; } = 900;
    public int? MaxPasswordAgeDays { get; init; } = null;
    public bool EnableDictionaryCheck { get; init; } = true;

    /// <summary>
    /// Minimum gerekli parola entropy deðeri (bits).
    /// null = entropy kontrolü devre dýþý
    /// </summary>
    public double? MinEntropyBits { get; init; }

    public PasswordHashOptions Hash { get; init; } = new();
}
