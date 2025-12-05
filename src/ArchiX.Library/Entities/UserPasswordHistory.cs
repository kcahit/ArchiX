namespace ArchiX.Library.Entities;

/// <summary>
/// Kullanýcýnýn geçmiþ parolalarýný saklar (RL-02).
/// historyCount kadar kayýt tutulur, eski kayýtlar silinir.
/// </summary>
public sealed class UserPasswordHistory : BaseEntity
{
    /// <summary>
    /// Kullanýcý FK (User.Id).
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Parolanýn hash'i (Argon2id encoded string).
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Hash algoritmasý adý (örn: Argon2id).
    /// </summary>
    public required string HashAlgorithm { get; set; }

    /// <summary>
    /// Parolanýn oluþturulma zamaný.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}