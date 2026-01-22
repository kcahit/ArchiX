using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Parametre tanımı (master). (Group, Key) benzersizdir.
    /// Gerçek değerler ParameterApplication'da (detail) tutulur.
    /// </summary>
    [Index(nameof(Group), nameof(Key), IsUnique = true)]
    [Index(nameof(ParameterDataTypeId))]
    public sealed class Parameter : BaseEntity
    {
        [Required, MaxLength(75)]
        public string Group { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Key { get; set; } = null!;

        public int ParameterDataTypeId { get; set; }
        public ParameterDataType DataType { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>JSON template/örnek değer (opsiyonel)</summary>
        public string? Template { get; set; }

        // Navigation
        public ICollection<ParameterApplication> Applications { get; set; } = [];
    }
}
