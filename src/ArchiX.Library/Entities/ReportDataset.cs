using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Dataset definition stored in _ArchiX (ArchiX.Library).
    /// </summary>
    [Index(nameof(ReportDatasetTypeId))]
    [Index(nameof(DisplayName))]
    public sealed class ReportDataset : BaseEntity
    {
        public int ReportDatasetTypeId { get; set; }
        public ReportDatasetType Type { get; set; } = null!;

        /// <summary>
        /// DB datasetlerde zorunlu: connection alias. File datasetlerde null olmalıdır.
        /// </summary>
        [MaxLength(100)]
        public string? ConnectionName { get; set; }

        /// <summary>
        /// DB: object name (sp/view/table). File: filename (json/xls/...).
        /// </summary>
        [Required, MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// UI dropdown'da görünen isim.
        /// </summary>
        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// File dataset path traversal riskini azaltmak için: Root (parametre) + SubPath (dataset) + FileName.
        /// Root parametre tablosundan gelecek (İş #3).
        /// </summary>
        [MaxLength(260)]
        public string? SubPath { get; set; }
    }
}
