namespace ArchiX.Library.Abstractions.Localization;

/// <summary>
/// Çok dilli destek için display name ve listeleme hizmetlerini tanýmlar.
/// </summary>
public interface IDisplayNameService
{
 Task<string?> GetDisplayNameAsync(string itemType,string entityName,string fieldName,string code,string culture,CancellationToken cancellationToken = default);
 Task<List<(int Id, string DisplayName)>> GetListAsync(string itemType,string entityName,string fieldName,string culture,CancellationToken cancellationToken = default);
}
