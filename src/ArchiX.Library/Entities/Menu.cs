using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArchiX.Library.Entities
{
    /// <summary>Menü/navigation kaydı (müşteri DB'de tutulur).</summary>
    [Table("Menus")]
    public sealed class Menu : BaseEntity
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(300)]
        public string? Url { get; set; }

        public int SortOrder { get; set; }

        public int? ParentId { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }
    }
}
