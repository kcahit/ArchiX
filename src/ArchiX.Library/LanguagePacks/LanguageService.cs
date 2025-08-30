using Microsoft.EntityFrameworkCore;
using ArchiX.Library.Context;

namespace ArchiX.Library.LanguagePacks
{
    public class LanguageService : ILanguageService
    {
        private readonly AppDbContext _db;

        public LanguageService(AppDbContext db)
        {
            _db = db;
        }

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
