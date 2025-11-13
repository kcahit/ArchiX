using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Entities
{
    /// <summary>DEPRECATED: Yerine 'Parameter' ve 'ParameterDataType' kullanın.</summary>
    [Obsolete("DEPRECATED: Use 'Parameter' and 'ParameterDataType'. This type is excluded from EF model.")]
    public class ArchiXSetting : BaseEntity
    {
        // EF şemasından hariç tut.
        public new static readonly bool MapToDb = false;

        [Required, MaxLength(200)]
        public string Key { get; set; } = null!;

        public string Value { get; set; } = null!;

        [MaxLength(50)]
        public string? Group { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
