using ArchiX.Library.Entities;

namespace ArchiX.Library.LanguagePacks
{
    /// <summary>
    /// Çok dillilik için tanımlar.
    /// Herhangi bir tablo/kolon veya sistem kodu için çoklu dil desteği sağlar.
    /// </summary>
    public class LanguagePack : BaseEntity
    {
        /// <summary>
        /// Kaynağın tipi (örn: Operator, Entity, Field, Status, Enum)
        /// </summary>
        public string ItemType { get; set; } = default!;

        /// <summary>
        /// İlgili entity veya tablo adı (örn: Product, FilterItem)
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// İlgili alan adı (örn: Name, StatusId, Code)
        /// </summary>
        public string? FieldName { get; set; }

        /// <summary>
        /// Değer kodu (örn: Equals, NotBetween, 1, 3)
        /// </summary>
        public string Code { get; set; } = default!;

        /// <summary>
        /// Dil kodu (örn: tr-TR, en-US)
        /// </summary>
        public string Culture { get; set; } = default!;

        /// <summary>
        /// Kullanıcıya gösterilecek metin
        /// </summary>
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Açıklama/yardım metni
        /// </summary>
        public string? Description { get; set; }
    }
}
