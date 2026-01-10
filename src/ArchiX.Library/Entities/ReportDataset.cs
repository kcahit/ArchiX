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
        /// DB: Sql cümlesi yazılacak. File: Mutseriler.ndjson gibi (json/xls/...).
        /// </summary>
        [Required, MaxLength(3000)]
        public string Source { get; set; } = string.Empty;

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

        /// <summary>
        /// Dataset input parametre şeması (JSON). UI'da kullanıcıdan alınacak input parametrelerini tanımlar.
        /// Format: array. Örn:
        /// [
        ///   { "name": "@StartDate", "type": "DateTime" },
        ///   { "name": "@CustomerCode", "type": "NVarchar(50)" }
        /// ]
        /// </summary>
        /// 
        [MaxLength(2000)]
        public string? InputParameter { get; set; }

        /// <summary>
        /// Dataset output parametre şeması (JSON). Varsayılan davranış: UI'da input üretmez (bilgi amaçlı).
        /// </summary>
        ///
        [MaxLength(2000)]
        public string? OutputParameter { get; set; }
    }
}
