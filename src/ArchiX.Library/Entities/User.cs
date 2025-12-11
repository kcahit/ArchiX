using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    [Index(nameof(NormalizedUserName), IsUnique = true)]
    [Index(nameof(NormalizedEmail))]
    public sealed class User : BaseEntity
    {
        [Required, MaxLength(100)]
        public string UserName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string NormalizedUserName { get; set; } = null!;

        [MaxLength(200)]
        public string? DisplayName { get; set; }

        [MaxLength(256)]
        public string? Email { get; set; }

        [MaxLength(256)]
        public string? NormalizedEmail { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public bool IsAdmin { get; set; }

        /// <summary>
        /// Son parola deðiþim zamaný (RL-04: Aging kontrolü için).
        /// Null = Parola hiç deðiþtirilmedi veya yaþlandýrma takibi yok.
        /// </summary>
        public DateTimeOffset? PasswordChangedAtUtc { get; set; }

        /// <summary>
        /// Kullanýcýya özel parola yaþlandýrma süresi (gün).
        /// Null = Genel politika (Parameters.MaxPasswordAgeDays) kullanýlýr.
        /// Örnek: 90 = 90 gün sonra zorunlu deðiþim.
        /// </summary>
        public int? MaxPasswordAgeDays { get; set; }=90;
    }
}
