namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Yaygýn kelime sözlüðü kontrolü (dictionary attack korumasý)
/// </summary>
public interface IPasswordDictionaryChecker
{
    /// <summary>
    /// Parolanýn yaygýn kelime listesinde olup olmadýðýný kontrol eder
    /// </summary>
    Task<bool> IsCommonPasswordAsync(string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sözlükteki toplam kelime sayýsýný döner
    /// </summary>
    int GetDictionaryWordCount();
}
