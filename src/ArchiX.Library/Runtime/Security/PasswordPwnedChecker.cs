using System.Security.Cryptography;
using System.Text;

using ArchiX.Library.Abstractions.Security;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Have I Been Pwned API ile parola sýzýntý kontrolü (k-Anonymity).
/// </summary>
public sealed class PasswordPwnedChecker : IPasswordPwnedChecker
{
    private const string HibpApiUrl = "https://api.pwnedpasswords.com/range/";
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PasswordPwnedChecker> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public PasswordPwnedChecker(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<PasswordPwnedChecker> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;

        // HIBP API için User-Agent zorunlu
        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ArchiX-PasswordPolicy/1.0");
    }

    public async Task<bool> IsPwnedAsync(string password, CancellationToken ct = default)
    {
        var count = await GetPwnedCountAsync(password, ct).ConfigureAwait(false);
        return count > 0;
    }

    public async Task<int> GetPwnedCountAsync(string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            return 0;

        // SHA-1 hash hesapla
        var hash = ComputeSha1(password);
        var prefix = hash[..5]; // Ýlk 5 karakter
        var suffix = hash[5..]; // Kalan kýsým

        // Cache kontrolü
        var cacheKey = $"pwned:{prefix}";
        if (_cache.TryGetValue<Dictionary<string, int>>(cacheKey, out var cachedResults))
        {
            _logger.LogDebug("HIBP cache hit for prefix: {Prefix}", prefix);
            return cachedResults!.TryGetValue(suffix, out var count) ? count : 0;
        }

        // API çaðrýsý
        try
        {
            var response = await _httpClient.GetStringAsync($"{HibpApiUrl}{prefix}", ct).ConfigureAwait(false);
            var results = ParseHibpResponse(response);

            // Cache'e kaydet
            _cache.Set(cacheKey, results, _cacheDuration);

            _logger.LogInformation("HIBP API called for prefix: {Prefix}, found {Count} hashes", prefix, results.Count);

            return results.TryGetValue(suffix, out var pwnedCount) ? pwnedCount : 0;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HIBP API çaðrýsý baþarýsýz (prefix: {Prefix})", prefix);
            return 0; // Hata durumunda güvenli kabul et (fail-open)
        }
    }

    private static string ComputeSha1(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA1.HashData(bytes);
        return Convert.ToHexString(hash); // Büyük harf
    }

    private static Dictionary<string, int> ParseHibpResponse(string response)
    {
        var results = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in response.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var count))
            {
                results[parts[0].Trim()] = count;
            }
        }

        return results;
    }
}
