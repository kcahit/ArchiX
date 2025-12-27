using System.Text.Json;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// PasswordPolicy JSON þemasýnýn server-side doðrulamasý (PK-03).
/// Tüm required alanlarý ve tür kontrollerini yapar.
/// </summary>
public static class PasswordPolicySchemaValidator
{
    /// <summary>
    /// JSON'ý parse eder ve þema kurallarýna uygunluðunu kontrol eder.
    /// </summary>
    /// <returns>Hata varsa liste, yoksa boþ liste.</returns>
    public static IReadOnlyList<string> Validate(string json)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("JSON boþ olamaz.");
            return errors;
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            errors.Add($"JSON parse hatasý: {ex.Message}");
            return errors;
        }

        var root = doc.RootElement;

        // version (required, number)
        if (!root.TryGetProperty("version", out var vProp) || vProp.ValueKind != JsonValueKind.Number)
            errors.Add("'version' alaný zorunlu ve sayý olmalý.");

        // minLength (required, number, >= 1)
        if (!root.TryGetProperty("minLength", out var minLenProp) || minLenProp.ValueKind != JsonValueKind.Number)
            errors.Add("'minLength' alaný zorunlu ve sayý olmalý.");
        else if (minLenProp.GetInt32() < 1)
            errors.Add("'minLength' en az 1 olmalý.");

        // maxLength (required, number, >= minLength)
        if (!root.TryGetProperty("maxLength", out var maxLenProp) || maxLenProp.ValueKind != JsonValueKind.Number)
            errors.Add("'maxLength' alaný zorunlu ve sayý olmalý.");
        else if (minLenProp.ValueKind == JsonValueKind.Number && maxLenProp.GetInt32() < minLenProp.GetInt32())
            errors.Add("'maxLength' 'minLength'ten küçük olamaz.");

        // requireUpper, requireLower, requireDigit, requireSymbol (required, boolean)
        ValidateBooleanField(root, "requireUpper", errors);
        ValidateBooleanField(root, "requireLower", errors);
        ValidateBooleanField(root, "requireDigit", errors);
        ValidateBooleanField(root, "requireSymbol", errors);

        // allowedSymbols (required, string)
        if (!root.TryGetProperty("allowedSymbols", out var symProp) || symProp.ValueKind != JsonValueKind.String)
            errors.Add("'allowedSymbols' alaný zorunlu ve string olmalý.");

        // minDistinctChars (required, number, >= 0)
        ValidateNonNegativeNumber(root, "minDistinctChars", errors);

        // maxRepeatedSequence (required, number, >= 0)
        ValidateNonNegativeNumber(root, "maxRepeatedSequence", errors);

        // blockList (required, array)
        if (!root.TryGetProperty("blockList", out var blockProp) || blockProp.ValueKind != JsonValueKind.Array)
            errors.Add("'blockList' alaný zorunlu ve array olmalý.");

        // historyCount (required, number, >= 0)
        ValidateNonNegativeNumber(root, "historyCount", errors);

        // lockoutThreshold (required, number, >= 0)
        ValidateNonNegativeNumber(root, "lockoutThreshold", errors);

        // lockoutSeconds (required, number, >= 0)
        ValidateNonNegativeNumber(root, "lockoutSeconds", errors);

        // hash (required, object)
        if (!root.TryGetProperty("hash", out var hashProp) || hashProp.ValueKind != JsonValueKind.Object)
        {
            errors.Add("'hash' alaný zorunlu ve object olmalý.");
        }
        else
        {
            // hash.algorithm (required, string)
            if (!hashProp.TryGetProperty("algorithm", out var algoProp) || algoProp.ValueKind != JsonValueKind.String)
                errors.Add("'hash.algorithm' alaný zorunlu ve string olmalý.");

            // hash.memoryKb, parallelism, iterations, saltLength, hashLength (required, number, > 0)
            ValidatePositiveNumber(hashProp, "memoryKb", errors);
            ValidatePositiveNumber(hashProp, "parallelism", errors);
            ValidatePositiveNumber(hashProp, "iterations", errors);
            ValidatePositiveNumber(hashProp, "saltLength", errors);
            ValidatePositiveNumber(hashProp, "hashLength", errors);

            // hash.fallback (required, object)
            if (!hashProp.TryGetProperty("fallback", out var fbProp) || fbProp.ValueKind != JsonValueKind.Object)
            {
                errors.Add("'hash.fallback' alaný zorunlu ve object olmalý.");
            }
            else
            {
                if (!fbProp.TryGetProperty("algorithm", out var fbAlgoProp) || fbAlgoProp.ValueKind != JsonValueKind.String)
                    errors.Add("'hash.fallback.algorithm' alaný zorunlu ve string olmalý.");

                ValidatePositiveNumber(fbProp, "iterations", errors);
            }

            // hash.pepperEnabled (required, boolean)
            ValidateBooleanField(hashProp, "pepperEnabled", errors);
        }

        return errors;
    }

    /// <summary>
    /// JSON'ý doðrular, hata varsa exception fýrlatýr.
    /// </summary>
    public static void ValidateOrThrow(string json)
    {
        var errors = Validate(json);
        if (errors.Count > 0)
            throw new InvalidOperationException($"PasswordPolicy þema hatasý: {string.Join("; ", errors)}");
    }

    private static void ValidateBooleanField(JsonElement element, string fieldName, List<string> errors)
    {
        if (!element.TryGetProperty(fieldName, out var prop) ||
            (prop.ValueKind != JsonValueKind.True && prop.ValueKind != JsonValueKind.False))
        {
            errors.Add($"'{fieldName}' alaný zorunlu ve boolean olmalý.");
        }
    }

    private static void ValidateNonNegativeNumber(JsonElement element, string fieldName, List<string> errors)
    {
        if (!element.TryGetProperty(fieldName, out var prop) || prop.ValueKind != JsonValueKind.Number)
        {
            errors.Add($"'{fieldName}' alaný zorunlu ve sayý olmalý.");
        }
        else if (prop.GetInt32() < 0)
        {
            errors.Add($"'{fieldName}' negatif olamaz.");
        }
    }

    private static void ValidatePositiveNumber(JsonElement element, string fieldName, List<string> errors)
    {
        if (!element.TryGetProperty(fieldName, out var prop) || prop.ValueKind != JsonValueKind.Number)
        {
            errors.Add($"'{fieldName}' alaný zorunlu ve sayý olmalý.");
        }
        else if (prop.GetInt32() <= 0)
        {
            errors.Add($"'{fieldName}' pozitif olmalý.");
        }
    }
}
