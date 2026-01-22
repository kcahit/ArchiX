using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Parametre değeri (detail). Her parametre birden fazla uygulamada farklı değer alabilir.
    /// (ParameterId, ApplicationId) benzersizdir.
    /// </summary>
    [Index(nameof(ParameterId), nameof(ApplicationId), IsUnique = true)]
    [Index(nameof(ApplicationId))]
    public sealed class ParameterApplication : BaseEntity
    {
        public int ParameterId { get; set; }
        public Parameter Parameter { get; set; } = null!;

        public int ApplicationId { get; set; }
        public Application Application { get; set; } = null!;

        /// <summary>Parametre değeri (JSON veya primitive string)</summary>
        [Required]
        public string Value { get; set; } = null!;

        /// <summary>Optimistic concurrency token</summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];
    }
}
