namespace ArchiX.Library.Abstractions.Time;

/// <summary>
/// Sunucu zamaný için soyutlama.
/// </summary>
public interface IClock
{
 DateTimeOffset UtcNow { get; }
}
