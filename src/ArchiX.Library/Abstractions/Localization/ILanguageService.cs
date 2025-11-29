using System.Globalization;

namespace ArchiX.Library.Abstractions.Localization;

/// <summary>
/// Çok dillilik için sözlük tabanlý servis sözleþmesi.
/// </summary>
public interface ILanguageService
{
 CultureInfo CurrentCulture { get; set; }
 string T(string key, bool throwIfMissing = false);
 string T(string key, params object[] args);
 void Set(string key, string value);
 bool TryGet(string key, out string value);
 Task<string?> GetDisplayNameAsync(string itemType,string entityName,string fieldName,string code,string culture,CancellationToken cancellationToken = default);
 Task<List<(int Id, string DisplayName)>> GetListAsync(string itemType,string entityName,string fieldName,string culture,CancellationToken cancellationToken = default);
}
