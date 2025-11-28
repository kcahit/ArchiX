using System.Text.Json;

namespace ArchiX.Library.Formatting;

public static class JsonTextFormatter
{
    private static readonly JsonSerializerOptions PrettyOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions MinifyOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static string Pretty(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        return JsonSerializer.Serialize(doc.RootElement, PrettyOptions);
    }

    public static string Minify(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        return JsonSerializer.Serialize(doc.RootElement, MinifyOptions);
    }

    public static bool TryValidate(string rawJson, out string? error)
    {
        try
        {
            using var _ = JsonDocument.Parse(rawJson);
            error = null;
            return true;
        }
        catch (JsonException ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
