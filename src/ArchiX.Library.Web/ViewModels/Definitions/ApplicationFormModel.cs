using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Web.ViewModels.Definitions;

public sealed class ApplicationFormModel
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string DefaultCulture { get; set; } = "tr-TR";

    [StringLength(100)]
    public string? TimeZoneId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
