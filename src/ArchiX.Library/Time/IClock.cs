namespace ArchiX.Library.Time;

/// <summary>
/// Sunucu zamanı için soyutlama.
/// Test edilebilirlik ve farklı zaman sağlayıcıları kullanmak için uygulanır.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Geçerli UTC zamanı.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}

/// <summary>
/// Varsayılan saat sağlayıcı.
/// Doğrudan <see cref="DateTimeOffset.UtcNow"/> döndürür.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
