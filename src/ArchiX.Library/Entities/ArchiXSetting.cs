using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Entities
{
    /// <summary>Key/Value hierarchical settings table (precedence: Env > DB > appsettings).</summary>
    public class ArchiXSetting
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Key { get; set; } = null!;

        public string Value { get; set; } = null!;

        [MaxLength(50)]
        public string? Group { get; set; }

        public bool IsProtected { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
