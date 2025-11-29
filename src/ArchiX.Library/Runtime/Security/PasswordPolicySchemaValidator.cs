using System.Text.Json;

namespace ArchiX.Library.Runtime.Security;

internal static class PasswordPolicySchemaValidator
{
    // Zorunlu alanlar listesi (kök seviyede)
    private static readonly string[] RequiredRootProps =
    {
        "version",
        "minLength",
        "maxLength",
        "requireUpper",
        "requireLower",
        "requireDigit",
        "requireSymbol",
        "allowedSymbols",
        "minDistinctChars",
        "maxRepeatedSequence",
        "blockList",
        "historyCount",
        "lockoutThreshold",
        "lockoutSeconds",
        "hash"
    };

    // Hash alt nesnesi için zorunlu alanlar
    private static readonly string[] RequiredHashProps =
    {
        "algorithm",
        "memoryKb",
        "parallelism",
        "iterations",
        "saltLength",
        "hashLength",
        "fallback",
        "pepperEnabled"
    };

    // Fallback alt nesnesi için zorunlu alanlar
    private static readonly string[] RequiredFallbackProps =
    {
        "algorithm",
        "iterations"
    };

    public static void ValidateOrThrow(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            throw new InvalidOperationException("PasswordPolicy JSON boþ olamaz.");

        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("PasswordPolicy JSON bir nesne olmalýdýr.");

        // Kök zorunlu alanlar
        foreach (var prop in RequiredRootProps)
        {
            if (!root.TryGetProperty(prop, out var el))
                throw new InvalidOperationException($"Eksik zorunlu alan: {prop}");
        }

        // Tür kontrolleri ve basit aralýklar
        RequireNumber(root, "version", min: 1);
        var minLen = RequireNumber(root, "minLength", min: 1);
        var maxLen = RequireNumber(root, "maxLength", min: minLen);
        RequireBool(root, "requireUpper");
        RequireBool(root, "requireLower");
        RequireBool(root, "requireDigit");
        RequireBool(root, "requireSymbol");
        RequireString(root, "allowedSymbols");
        RequireNumber(root, "minDistinctChars", min: 0);
        RequireNumber(root, "maxRepeatedSequence", min: 0);
        RequireArray(root, "blockList");
        RequireNumber(root, "historyCount", min: 0);
        RequireNumber(root, "lockoutThreshold", min: 0);
        RequireNumber(root, "lockoutSeconds", min: 0);

        // hash nesnesi
        var hash = root.GetProperty("hash");
        foreach (var prop in RequiredHashProps)
        {
            if (!hash.TryGetProperty(prop, out var _))
                throw new InvalidOperationException($"Eksik zorunlu alan: hash.{prop}");
        }

        RequireString(hash, "algorithm");
        RequireNumber(hash, "memoryKb", min: 4096); // 4MB altý anlamsýz
        RequireNumber(hash, "parallelism", min: 1);
        RequireNumber(hash, "iterations", min: 1);
        RequireNumber(hash, "saltLength", min: 8);
        RequireNumber(hash, "hashLength", min: 16);
        RequireBool(hash, "pepperEnabled");

        var fallback = hash.GetProperty("fallback");
        foreach (var prop in RequiredFallbackProps)
        {
            if (!fallback.TryGetProperty(prop, out var _))
                throw new InvalidOperationException($"Eksik zorunlu alan: hash.fallback.{prop}");
        }
        RequireString(fallback, "algorithm");
        RequireNumber(fallback, "iterations", min: 100000);

        // allowedSymbols ile requireSymbol tutarlýlýðý
        var requireSymbol = root.GetProperty("requireSymbol").GetBoolean();
        var allowedSymbols = root.GetProperty("allowedSymbols").GetString() ?? string.Empty;
        if (requireSymbol && string.IsNullOrWhiteSpace(allowedSymbols))
            throw new InvalidOperationException("requireSymbol=true ise allowedSymbols boþ olamaz.");

        // min/max mantýðý
        if (minLen > maxLen)
            throw new InvalidOperationException("minLength, maxLength deðerinden büyük olamaz.");
    }

    private static int RequireNumber(JsonElement obj, string name, int? min = null, int? max = null)
    {
        if (!obj.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.Number)
            throw new InvalidOperationException($"Alan '{name}' number olmalýdýr.");
        if (!el.TryGetInt32(out var val))
            throw new InvalidOperationException($"Alan '{name}' integer olmalýdýr.");
        if (min is not null && val < min)
            throw new InvalidOperationException($"Alan '{name}' en az {min} olmalýdýr.");
        if (max is not null && val > max)
            throw new InvalidOperationException($"Alan '{name}' en çok {max} olmalýdýr.");
        return val;
    }

    private static void RequireBool(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.True && el.ValueKind != JsonValueKind.False)
            throw new InvalidOperationException($"Alan '{name}' boolean olmalýdýr.");
    }

    private static void RequireString(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException($"Alan '{name}' string olmalýdýr.");
    }

    private static void RequireArray(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException($"Alan '{name}' array olmalýdýr.");
    }
}
