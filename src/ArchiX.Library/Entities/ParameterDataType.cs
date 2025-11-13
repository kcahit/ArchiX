using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>Parametre deðer tipi tanýmý (DB tablosu).</summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(Name), IsUnique = true)]
    public sealed class ParameterDataType : BaseEntity
    {
        /// <summary>Tür kodu (gruplar: NVARCHAR 1–100, Numeric 200+, Temporal 300+, Other 900+).</summary>
        public int Code { get; set; }

        /// <summary>Benzersiz ad (örn. NVarChar_50, Int, Date, Json).</summary>
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>Küme adý (örn. NVarChar, Numeric, Temporal, Other).</summary>
        [MaxLength(20)]
        public string? Category { get; set; }

        /// <summary>Açýklama (örn. NVARCHAR mantýksal uzunluk, format vb.).</summary>
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}