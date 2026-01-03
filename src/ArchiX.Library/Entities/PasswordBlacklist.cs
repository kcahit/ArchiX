namespace ArchiX.Library.Entities;

/// <summary>
/// Parola politikasý için dinamik blacklist (yasaklý kelimeler).
/// ApplicationId bazýnda ayrý yönetim, soft-delete desteði.
/// </summary>
public sealed class PasswordBlacklist : BaseEntity
{
    public required int ApplicationId { get; set; }

    public required string Word { get; set; }

    // Navigation (virtual kaldýrýldý - sealed class'ta kullanýlamaz)
    public Application? Application { get; set; }
}
