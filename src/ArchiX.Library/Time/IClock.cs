namespace ArchiX.Library.Time;

public sealed class SystemClock : ArchiX.Library.Abstractions.Time.IClock
{
 public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
