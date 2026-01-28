using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>Uygulama (tenant/proje) tanımı.</summary>
    [Index(nameof(Code), IsUnique = true)]
    public sealed class Application : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        // Zorunlu varsayılan kültür (örn. tr-TR)
        [Required, MaxLength(10)]
        public string DefaultCulture { get; set; } = "tr-TR";

        [Required, MaxLength(100)]
        public string TimeZoneId { get; set; } = "Europe/Istanbul";

        [MaxLength(500)]
        public string? Description { get; set; }

        public int ConfigVersion { get; set; } = 1;

        [MaxLength(100)]
        public string? ExternalKey { get; set; }
    }
}
