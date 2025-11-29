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

        public int ApplicationId { get; set; }
        public Application Application { get; set; } = null!;

        public int ParameterDataTypeId { get; set; }
        public ParameterDataType DataType { get; set; } = null!;

        public string? Value { get; set; }
        public string? Template { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Concurrency kontrolü (PK-06)
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
