// src/ArchiX.Library.Web/ViewModels/Security/BlacklistViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.Web.ViewModels.Security;

public sealed class BlacklistViewModel
{
    public int ApplicationId { get; init; }

    public IReadOnlyList<BlacklistWordViewModel> Words { get; init; } = Array.Empty<BlacklistWordViewModel>();

    [Display(Name = "Kelime Ekle")]
    [MaxLength(256)]
    public string? NewWord { get; set; }

    [Display(Name = "Toplu Ekle (her satýr bir kelime)")]
    public string? BulkWords { get; set; }
}

public sealed class BlacklistWordViewModel
{
    public int Id { get; init; }

    public string Word { get; init; } = string.Empty;

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; init; }

    public bool IsActive { get; init; }
}
