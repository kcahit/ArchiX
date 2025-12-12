// src/ArchiX.Library.Web/ViewModels/Security/PolicySettingsViewModel.cs
using System.ComponentModel.DataAnnotations;

using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Web.ViewModels.Security;

public sealed class PolicySettingsViewModel
{
    public int Version { get; set; } = 1;

    [Range(8, 256)]
    public int MinLength { get; set; }

    [Range(8, 256)]
    public int MaxLength { get; set; }

    public bool RequireUpper { get; set; }

    public bool RequireLower { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireSymbol { get; set; }

    [MaxLength(64)]
    public string AllowedSymbols { get; set; } = string.Empty;

    [Range(1, 20)]
    public int MinDistinctChars { get; set; }

    [Range(1, 10)]
    public int MaxRepeatedSequence { get; set; }

    [Range(0, 25)]
    public int HistoryCount { get; set; }

    [Range(1, 3650)]
    public int? MaxPasswordAgeDays { get; set; }

    [Range(1, 50)]
    public int LockoutThreshold { get; set; }

    [Range(30, 86400)]
    public int LockoutSeconds { get; set; }

    public string[] BlockList { get; set; } = Array.Empty<string>();

    [Display(Name = "Blacklist (her satýra bir kelime)")]
    public string BlockListRaw { get; set; } = string.Empty;

    public PasswordHashSettingsViewModel Hash { get; set; } = new();

    public static PolicySettingsViewModel FromOptions(PasswordPolicyOptions options) =>
        new()
        {
            Version = options.Version,
            MinLength = options.MinLength,
            MaxLength = options.MaxLength,
            RequireUpper = options.RequireUpper,
            RequireLower = options.RequireLower,
            RequireDigit = options.RequireDigit,
            RequireSymbol = options.RequireSymbol,
            AllowedSymbols = options.AllowedSymbols,
            MinDistinctChars = options.MinDistinctChars,
            MaxRepeatedSequence = options.MaxRepeatedSequence,
            HistoryCount = options.HistoryCount,
            MaxPasswordAgeDays = options.MaxPasswordAgeDays,
            LockoutThreshold = options.LockoutThreshold,
            LockoutSeconds = options.LockoutSeconds,
            BlockList = options.BlockList,
            BlockListRaw = string.Join(Environment.NewLine, options.BlockList ?? Array.Empty<string>()),
            Hash = PasswordHashSettingsViewModel.FromOptions(options.Hash)
        };

    public PasswordPolicyOptions ToOptions()
    {
        var blockList = ParseBlockList(BlockListRaw);
        BlockList = blockList;

        return new PasswordPolicyOptions
        {
            Version = Version,
            MinLength = MinLength,
            MaxLength = MaxLength,
            RequireUpper = RequireUpper,
            RequireLower = RequireLower,
            RequireDigit = RequireDigit,
            RequireSymbol = RequireSymbol,
            AllowedSymbols = AllowedSymbols ?? string.Empty,
            MinDistinctChars = MinDistinctChars,
            MaxRepeatedSequence = MaxRepeatedSequence,
            BlockList = blockList,
            HistoryCount = HistoryCount,
            LockoutThreshold = LockoutThreshold,
            LockoutSeconds = LockoutSeconds,
            MaxPasswordAgeDays = MaxPasswordAgeDays,
            Hash = Hash.ToOptions()
        };
    }

    private static string[] ParseBlockList(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<string>();

        return raw
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim())
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

public sealed class PasswordHashSettingsViewModel
{
    [Required]
    public string Algorithm { get; set; } = "Argon2id";

    [Range(4096, 262144)]
    public int MemoryKb { get; set; }

    [Range(1, 8)]
    public int Parallelism { get; set; }

    [Range(1, 10)]
    public int Iterations { get; set; }

    [Range(8, 64)]
    public int SaltLength { get; set; }

    [Range(16, 128)]
    public int HashLength { get; set; }

    public bool PepperEnabled { get; set; }

    public PasswordHashFallbackSettingsViewModel Fallback { get; set; } = new();

    public static PasswordHashSettingsViewModel FromOptions(PasswordHashOptions hash) =>
        new()
        {
            Algorithm = hash.Algorithm,
            MemoryKb = hash.MemoryKb,
            Parallelism = hash.Parallelism,
            Iterations = hash.Iterations,
            SaltLength = hash.SaltLength,
            HashLength = hash.HashLength,
            PepperEnabled = hash.PepperEnabled,
            Fallback = PasswordHashFallbackSettingsViewModel.FromOptions(hash.Fallback)
        };

    public PasswordHashOptions ToOptions() => new()
    {
        Algorithm = Algorithm,
        MemoryKb = MemoryKb,
        Parallelism = Parallelism,
        Iterations = Iterations,
        SaltLength = SaltLength,
        HashLength = HashLength,
        PepperEnabled = PepperEnabled,
        Fallback = Fallback.ToOptions()
    };
}

public sealed class PasswordHashFallbackSettingsViewModel
{
    [Required]
    public string Algorithm { get; set; } = "PBKDF2-SHA512";

    [Range(10000, 500000)]
    public int Iterations { get; set; }

    public static PasswordHashFallbackSettingsViewModel FromOptions(PasswordHashFallbackOptions fallback) =>
        new()
        {
            Algorithm = fallback.Algorithm,
            Iterations = fallback.Iterations
        };

    public PasswordHashFallbackOptions ToOptions() => new()
    {
        Algorithm = Algorithm,
        Iterations = Iterations
    };
}
