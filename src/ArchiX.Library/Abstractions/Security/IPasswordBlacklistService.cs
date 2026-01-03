namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Parola blacklist yönetimi ve kontrol servisi.
/// </summary>
public interface IPasswordBlacklistService
{
    /// <summary>
    /// Belirtilen kelimenin blacklist'te olup olmadýðýný kontrol eder.
    /// </summary>
    Task<bool> IsWordBlockedAsync(string word, int applicationId, CancellationToken ct = default);

    /// <summary>
    /// Belirtilen uygulama için tüm bloklanmýþ kelimeleri döner.
    /// </summary>
    Task<IReadOnlyList<string>> GetBlockedWordsAsync(int applicationId, CancellationToken ct = default);

    /// <summary>
    /// Blacklist'e yeni kelime ekler.
    /// </summary>
    Task<bool> AddWordAsync(string word, int applicationId, CancellationToken ct = default);

    /// <summary>
    /// Blacklist'ten kelime siler.
    /// </summary>
    Task<bool> RemoveWordAsync(string word, int applicationId, CancellationToken ct = default);

    /// <summary>
    /// Belirtilen uygulama için bloklanmýþ kelime sayýsýný döner.
    /// </summary>
    Task<int> GetCountAsync(int applicationId, CancellationToken ct = default);

    /// <summary>
    /// Belirtilen uygulama için cache'i temizler.
    /// </summary>
    void InvalidateCache(int applicationId);
}