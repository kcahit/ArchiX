using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Entities
{
    /// <summary>Key/Value hierarchical settings table (precedence: Env > DB > appsettings).</summary>
    public class ArchiXSetting : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Key { get; set; } = null!;

        public string Value { get; set; } = null!;

        [MaxLength(50)]
        public string? Group { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
