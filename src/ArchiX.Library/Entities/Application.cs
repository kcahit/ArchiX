using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>Uygulama (tenant/proje) tanýmý.</summary>
    [Index(nameof(Code), IsUnique = true)]
    public sealed class Application : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        // Zorunlu varsayýlan kültür (örn. tr-TR)
        [Required, MaxLength(10)]
        public string DefaultCulture { get; set; } = "tr-TR";

        [MaxLength(100)]
        public string? TimeZoneId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int ConfigVersion { get; set; } = 1;

        [MaxLength(100)]
        public string? ExternalKey { get; set; }
    }
}
