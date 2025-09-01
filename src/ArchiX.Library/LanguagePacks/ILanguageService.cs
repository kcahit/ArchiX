using System.Threading;

namespace ArchiX.Library.LanguagePacks
{
    /// <summary>
    /// Çok dilli destek için display name ve listeleme hizmetlerini tanımlar.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Belirtilen öğenin (entity alanı) display name bilgisini asenkron olarak getirir.
        /// </summary>
        /// <param name="itemType">Öğe tipi.</param>
        /// <param name="entityName">Entity adı.</param>
        /// <param name="fieldName">Alan adı.</param>
        /// <param name="code">Kod.</param>
        /// <param name="culture">Kültür (örn: tr-TR, en-US).</param>
        /// <param name="cancellationToken">İptal token.</param>
        /// <returns>Display name veya null.</returns>
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
        /// <param name="itemType">Öğe tipi.</param>
        /// <param name="entityName">Entity adı.</param>
        /// <param name="fieldName">Alan adı.</param>
        /// <param name="culture">Kültür.</param>
        /// <param name="cancellationToken">İptal token.</param>
        /// <returns>(Id, DisplayName) çiftlerinden oluşan liste.</returns>
        Task<List<(int Id, string DisplayName)>> GetListAsync(
            string itemType,
            string entityName,
            string fieldName,
            string culture,
            CancellationToken cancellationToken = default);
    }
}
