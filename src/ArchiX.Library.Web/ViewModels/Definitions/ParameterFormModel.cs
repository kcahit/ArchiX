using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Web.ViewModels.Definitions;

public sealed class ParameterFormModel
{
    [Required]
    [StringLength(75)]
    public string Group { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Veri tipi zorunlu.")]
    public int ParameterDataTypeId { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(4000)]
    public string? Value { get; set; }
}
