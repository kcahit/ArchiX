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
    }
}
