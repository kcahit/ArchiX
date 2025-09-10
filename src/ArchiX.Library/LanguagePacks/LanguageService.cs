using System.Globalization;

using ArchiX.Library.Context;

namespace ArchiX.Library.LanguagePacks
{
    /// <summary>
    /// Çok dillilik desteği için in-memory sözlük tabanlı dil servisidir.
    /// </summary>
    public sealed class LanguageService : ILanguageService
    {
        private readonly Dictionary<string, string> _dict = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Varsayılan kurucu. Boş sözlük ve geçerli kültürle başlatır.
        /// </summary>
        public LanguageService()
        {
            CurrentCulture = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Belirtilen kültürle başlatan kurucu.
        /// </summary>
        public LanguageService(CultureInfo culture)
        {
            CurrentCulture = culture;
        }

        /// <summary>
        /// Belirtilen seed sözlüğü ve kültürle başlatan kurucu.
        /// </summary>
        public LanguageService(Dictionary<string, string> seed, CultureInfo culture)
        {
            _dict = new Dictionary<string, string>(seed, StringComparer.OrdinalIgnoreCase);
            CurrentCulture = culture;
        }

        /// <summary>
        /// AppDbContext tabanlı dil servisidir.
        /// DB’den aktif (StatusId == 3) kayıtları hem "disp:" hem "list:" olarak yükler.
        /// </summary>
        public LanguageService(AppDbContext db)
        {
            CurrentCulture = CultureInfo.CurrentCulture;

            // Yalnızca aktif kayıtlar
            var query = db.Set<LanguagePack>().Where(x => x.StatusId == 3);

            foreach (var lp in query)
            {
                // Tekil görüntüleme anahtarı (display name look-up)
                var dispKey = $"disp:{lp.ItemType}:{lp.EntityName}:{lp.FieldName}:{lp.Code}:{lp.Culture}";
                _dict[dispKey] = lp.DisplayName ?? string.Empty;

                // Listeleme anahtarı (liste üretimi için; prefix ile eşleşecek)
                // Örn: list:Operator:FilterItem:Code:tr-TR:Equals
                var listPrefix = $"list:{lp.ItemType}:{lp.EntityName}:{lp.FieldName}:{lp.Culture}";
                var listKey = $"{listPrefix}:{lp.Code}";
                _dict[listKey] = lp.DisplayName ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public CultureInfo CurrentCulture { get; set; }

        /// <inheritdoc/>
        public string T(string key, bool throwIfMissing = false)
        {
            if (_dict.TryGetValue(key, out var value))
                return value;

            if (throwIfMissing)
                throw new KeyNotFoundException($"Anahtar bulunamadı: {key}");

            return key;
        }

        /// <inheritdoc/>
        public string T(string key, params object[] args)
        {
            var text = T(key);
            return string.Format(CurrentCulture, text, args);
        }

        /// <inheritdoc/>
        public void Set(string key, string value)
        {
            _dict[key] = value;
        }

        /// <inheritdoc/>
        public bool TryGet(string key, out string value)
        {
            return _dict.TryGetValue(key, out value!);
        }

        /// <inheritdoc/>
        public Task<string?> GetDisplayNameAsync(
            string itemType,
            string entityName,
            string fieldName,
            string code,
            string culture,
            CancellationToken cancellationToken = default)
        {
            var key = $"disp:{itemType}:{entityName}:{fieldName}:{code}:{culture}";
            if (_dict.TryGetValue(key, out var value))
                return Task.FromResult<string?>(value);

            return Task.FromResult<string?>(null);
        }

        /// <inheritdoc/>
        public Task<List<(int Id, string DisplayName)>> GetListAsync(
            string itemType,
            string entityName,
            string fieldName,
            string culture,
            CancellationToken cancellationToken = default)
        {
            // "list:{itemType}:{entityName}:{fieldName}:{culture}" ile başlayan tüm kayıtları topla
            var prefix = $"list:{itemType}:{entityName}:{fieldName}:{culture}";

            var list = _dict
                .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select((kv, index) => (Id: index + 1, DisplayName: kv.Value))
                .ToList();

            return Task.FromResult(list);
        }
    }
}
