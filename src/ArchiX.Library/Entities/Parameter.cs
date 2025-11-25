using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>Uygulama parametresi. (Group, Key, ApplicationId) benzersizdir.</summary>
    [Index(nameof(Group), nameof(Key), nameof(ApplicationId), IsUnique = true)]
    [Index(nameof(ParameterDataTypeId))]
    public sealed class Parameter : BaseEntity
    {
        [Required, MaxLength(75)]
        public string Group { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Key { get; set; } = null!;

        // Uygulama (tenant/proje) FK
        public int ApplicationId { get; set; }
        public Application Application { get; set; } = null!;

        // DataType (FK)
        public int ParameterDataTypeId { get; set; }
        public ParameterDataType DataType { get; set; } = null!;

        // Fiziksel olarak nvarchar(max). Tüm tipler için tek kolon.
        public string? Value { get; set; }

        // Özellikle JSON tipleri için örnek þablon.
        public string? Template { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
