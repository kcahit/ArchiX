using System.Reflection;
using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

public class PasswordDictionaryChecker : IPasswordDictionaryChecker
{
    private const string CacheKey = "PasswordDictionary_CommonWords";
    private const string ResourceName = "ArchiX.Library.Resources.common-passwords.txt";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly IMemoryCache _cache;
    private readonly ILogger<PasswordDictionaryChecker> _logger;

    public PasswordDictionaryChecker(
        IMemoryCache cache,
        ILogger<PasswordDictionaryChecker> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsCommonPasswordAsync(string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var dictionary = await GetDictionaryAsync(cancellationToken);
        var normalizedPassword = password.Trim().ToLowerInvariant();

        return dictionary.Contains(normalizedPassword);
    }

    public int GetDictionaryWordCount()
    {
        var dictionary = _cache.Get<HashSet<string>>(CacheKey);
        return dictionary?.Count ?? 0;
    }

    private async Task<HashSet<string>> GetDictionaryAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<HashSet<string>>(CacheKey, out var cached))
        {
            return cached!;
        }

        var dictionary = await LoadDictionaryFromResourceAsync(cancellationToken);

        _cache.Set(CacheKey, dictionary, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Priority = CacheItemPriority.Normal
        });

        _logger.LogInformation(
            "Password dictionary loaded: {WordCount} words, cached for {Duration}",
            dictionary.Count,
            CacheDuration);

        return dictionary;
    }

    private static async Task<HashSet<string>> LoadDictionaryFromResourceAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var dictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Embedded resource not found: {ResourceName}");
        }

        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            var word = line.Trim();
            if (!string.IsNullOrWhiteSpace(word))
            {
                dictionary.Add(word.ToLowerInvariant());
            }
        }

        return dictionary;
    }
}
