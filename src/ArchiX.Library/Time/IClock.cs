namespace ArchiX.Library.Time;

/// <summary>Sunucu zamanı için soyutlama.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>Varsayılan saat sağlayıcı.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
