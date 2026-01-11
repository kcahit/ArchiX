using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Dataset type family/group. (Db/File/Other)
    /// </summary>
    public sealed class ReportDatasetTypeGroup : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
