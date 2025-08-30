using ArchiX.Library.Entities;
using System.ComponentModel.DataAnnotations;


namespace ArchiX.Library.Filtering
{
    /// <summary>
    /// Enum yerine DB tabanlı filtre tanımları.
    /// Görsel metinler LanguagePacks tablosundan gelir.
    /// </summary>
    public class FilterItem : BaseEntity
    {
        /// <summary>
        /// Tip: Operator | ileride başka seçenekler olabilir diye ekledim
        /// </summary>
        [Required]   // ✅ zorunlu alan
        [MaxLength(50)]
        public string ItemType { get; set; } = default!;

        /// <summary>
        /// Operatör/alan kodu (örn: Equals, 1, 3, NotBetween)
        /// </summary>
        [Required]   // ✅ zorunlu alan
        [MaxLength(50)]
        public string Code { get; set; } = default!;
    }
}
