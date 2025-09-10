using System.Globalization;

namespace ArchiX.Library.LanguagePacks
{
    /// <summary>
    /// Çok dillilik için sözlük tabanlı servis sözleşmesi.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Geçerli kültür.
        /// </summary>
        CultureInfo CurrentCulture { get; set; }

        /// <summary>
        /// Anahtara karşılık gelen çeviriyi döndürür.
        /// </summary>
        string T(string key, bool throwIfMissing = false);

        /// <summary>
        /// Anahtara karşılık gelen formatlı çeviriyi döndürür.
        /// </summary>
        string T(string key, params object[] args);

        /// <summary>
        /// Yeni çeviri ekler veya günceller.
        /// </summary>
        void Set(string key, string value);

        /// <summary>
        /// Anahtar mevcutsa çeviriyi döndürür.
        /// </summary>
        bool TryGet(string key, out string value);

        /// <summary>
        /// Belirtilen kriterlere göre DisplayName döndürür.
        /// </summary>
        Task<string?> GetDisplayNameAsync(
            string itemType,
            string entityName,
            string fieldName,
            string code,
            string culture,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirtilen kriterlere göre aktif çeviri listesini döndürür.
        /// </summary>
        Task<List<(int Id, string DisplayName)>> GetListAsync(
            string itemType,
            string entityName,
            string fieldName,
            string culture,
            CancellationToken cancellationToken = default);
    }
}
