namespace ArchiX.Library.LanguagePacks
{
    /// <summary>
    /// Çok dilli destek için display name ve listeleme hizmetlerini tanımlar.
    /// </summary>
    public interface IDisplayNameService
    {
        /// <summary>
        /// Belirtilen öğenin (entity alanı) display name bilgisini asenkron olarak getirir.
        /// </summary>
        Task<string?> GetDisplayNameAsync(
            string itemType,
            string entityName,
            string fieldName,
            string code,
            string culture,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirtilen entity ve alan için display name listesi getirir.
        /// </summary>
        Task<List<(int Id, string DisplayName)>> GetListAsync(
            string itemType,
            string entityName,
            string fieldName,
            string culture,
            CancellationToken cancellationToken = default);
    }
}
