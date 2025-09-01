using Microsoft.EntityFrameworkCore;
using ArchiX.Library.Context;

namespace ArchiX.Library.LanguagePacks
{
    /// <summary>
    /// Çok dilli destek için display name ve listeleme işlemlerini veritabanı üzerinden sağlayan servis sınıfı.
    /// </summary>
    public class LanguageService : ILanguageService
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// <see cref="LanguageService"/> için kurucu metot.
        /// </summary>
        /// <param name="db">Uygulamanın veritabanı context nesnesi.</param>
        public LanguageService(AppDbContext db)
        {
            _db = db;
        }

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
        public async Task<string?> GetDisplayNameAsync(
            string itemType,
            string entityName,
            string fieldName,
            string code,
            string culture,
            CancellationToken cancellationToken = default)
        {
            return await _db.Set<LanguagePack>()
                .Where(x => x.ItemType == itemType
                         && x.EntityName == entityName
                         && x.FieldName == fieldName
                         && x.Code == code
                         && x.Culture == culture
                         && x.StatusId == 3)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Belirtilen entity ve alan için display name listesini getirir.
        /// </summary>
        /// <param name="itemType">Öğe tipi.</param>
        /// <param name="entityName">Entity adı.</param>
        /// <param name="fieldName">Alan adı.</param>
        /// <param name="culture">Kültür.</param>
        /// <param name="cancellationToken">İptal token.</param>
        /// <returns>(Id, DisplayName) çiftlerinden oluşan liste.</returns>
        public async Task<List<(int Id, string DisplayName)>> GetListAsync(
            string itemType,
            string entityName,
            string fieldName,
            string culture,
            CancellationToken cancellationToken = default)
        {
            return await _db.Set<LanguagePack>()
                .Where(x => x.ItemType == itemType
                         && x.EntityName == entityName
                         && x.FieldName == fieldName
                         && x.Culture == culture
                         && x.StatusId == 3)
                .Select(x => new ValueTuple<int, string>(x.Id, x.DisplayName))
                .ToListAsync(cancellationToken);
        }
    }
}
