namespace ArchiX.Library.Abstractions.Security
{
    public sealed record AttemptLimiterOptions
    {
        public int MaxAttempts { get; init; } = 5;
        public TimeSpan Window { get; init; } = TimeSpan.FromMinutes(5);
        public TimeSpan Cooldown { get; init; } = TimeSpan.FromMinutes(5);
    }
}