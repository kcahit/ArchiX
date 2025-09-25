// File: src/ArchiX.Library/Infrastructure/Http/ProblemDetailsReader.cs
#nullable enable
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ArchiX.Library.Infrastructure.Http;

/// <summary>RFC7807 ProblemDetails okuma ve tek satırlık özet üretme yardımcıları.</summary>
public static partial class ProblemDetailsReader
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiWs();

    /// <summary>
    /// HTTP yanıtından RFC7807 ProblemDetails okumayı dener.
    /// Yalnızca <c>application/problem+json</c> ya da JSON içinde ProblemDetails alanları varsa döner.
    /// Aksi halde <c>null</c>.
    /// </summary>
    public static async Task<HttpApiProblem?> TryReadAsync(HttpResponseMessage response, CancellationToken ct = default)
    {
        if (response?.Content is null) return null;

        var headers = response.Content.Headers;
        var isProblemContent = IsProblemContent(headers);
        var isJson = IsJsonContent(headers);

        if (!isProblemContent && !isJson)
            return null; // ne problem ne JSON → çık

        try
        {
            await using var s = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var node = await JsonNode.ParseAsync(s, cancellationToken: ct).ConfigureAwait(false);
            if (node is null) return null;

            // Plain JSON ise, ProblemDetails’ı andıran alan var mı kontrol et.
            if (!isProblemContent)
            {
                if (node is not JsonObject obj) return null;

                var hasPdKey = obj.Any(kv =>
                    kv.Key.Equals("type", StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.Equals("title", StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.Equals("status", StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.Equals("detail", StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.Equals("instance", StringComparison.OrdinalIgnoreCase));

                if (!hasPdKey) return null; // sıradan JSON → null
            }

            var problem = node.Deserialize<HttpApiProblem>(JsonOpts);
            if (problem is null) return null;

            // Extensions çıkar
            if (node is JsonObject root)
            {
                var ext = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in root)
                {
                    var k = kv.Key;
                    if (k.Equals("type", StringComparison.OrdinalIgnoreCase) ||
                        k.Equals("title", StringComparison.OrdinalIgnoreCase) ||
                        k.Equals("status", StringComparison.OrdinalIgnoreCase) ||
                        k.Equals("detail", StringComparison.OrdinalIgnoreCase) ||
                        k.Equals("instance", StringComparison.OrdinalIgnoreCase))
                        continue;

                    ext[k] = kv.Value?.Deserialize<object>(JsonOpts);
                }

                if (ext.Count > 0)
                    problem = problem with { Extensions = ext };
            }

            return problem;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Serbest metni tek satıra indirir ve gerekirse kısaltır.</summary>
    public static string ToOneLine(string? text, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var one = MultiWs().Replace(text, " ").Trim();
        if (maxLength <= 0) return string.Empty;
        if (one.Length <= maxLength) return one;

        return SmartTrim(one, maxLength) + "…";
    }

    /// <summary><see cref="HttpApiProblem"/> için tek satırlık özet üretir.</summary>
    public static string ToOneLine(HttpApiProblem p, int maxLength = 200)
    {
        var code = p.Status?.ToString() ?? "?";
        var title = string.IsNullOrWhiteSpace(p.Title) ? "-" : ToOneLine(p.Title, Math.Min(80, maxLength));
        var detail = ToOneLine(p.Detail, Math.Max(0, maxLength - (code.Length + title.Length + 4)));
        return $"{code} {title} | {detail}";
    }

    private static string SmartTrim(string s, int max)
    {
        if (s.Length <= max) return s;
        var cut = s[..max];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > max * 2 / 3) cut = cut[..lastSpace];
        return cut;
    }

    private static bool IsProblemContent(HttpContentHeaders h)
        => h.ContentType?.MediaType?.Equals("application/problem+json", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsJsonContent(HttpContentHeaders h)
        => h.ContentType?.MediaType?.EndsWith("/json", StringComparison.OrdinalIgnoreCase) == true
           || h.ContentType?.MediaType?.EndsWith("+json", StringComparison.OrdinalIgnoreCase) == true;
}
