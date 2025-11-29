using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities;

/// <summary>
/// Parola politikasý deðiþiklikleri için audit trail kaydý.
/// </summary>
[Index(nameof(ApplicationId))]
public sealed class PasswordPolicyAudit : BaseEntity
{
    /// <summary>Uygulama/Tenant kimliði.</summary>
    public int ApplicationId { get; set; }

    /// <summary>Güncellemeyi yapan kullanýcý (0 = sistem).</summary>
    public int UserId { get; set; }

    /// <summary>Önceki JSON (null ise ilk ekleme).</summary>
    [Required]
    public string OldJson { get; set; } = string.Empty;

    /// <summary>Yeni JSON.</summary>
    [Required]
    public string NewJson { get; set; } = string.Empty;

    /// <summary>Ýþlem UTC zamaný (CreatedAt zaten var, yedek kolonu).</summary>
    public DateTimeOffset ChangedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
