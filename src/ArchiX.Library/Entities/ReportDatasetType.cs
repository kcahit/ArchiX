using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Dataset type. (sp/view/table/json/xls/...)
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(ReportDatasetTypeGroupId))]
    public sealed class ReportDatasetType : BaseEntity
    {
        public int ReportDatasetTypeGroupId { get; set; }
        public ReportDatasetTypeGroup Group { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
