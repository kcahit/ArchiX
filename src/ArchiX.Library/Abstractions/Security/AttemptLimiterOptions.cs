namespace ArchiX.Library.Abstractions.Security
{
    /// <summary>#57 Güvenlik attempt limiter parametreleri (DB-driven).</summary>
    public sealed class AttemptLimiterOptions
    {
        /// <summary>Deneme penceresi (saniye). Varsayılan: 600 (10 dakika).</summary>
        public int Window { get; set; } = 600;

        /// <summary>Maksimum deneme sayısı. Varsayılan: 5.</summary>
        public int MaxAttempts { get; set; } = 5;

        /// <summary>Cooldown süresi (saniye). Varsayılan: 300 (5 dakika).</summary>
        public int CooldownSeconds { get; set; } = 300;

        /// <summary>Window'u TimeSpan olarak döndürür.</summary>
        public TimeSpan GetWindow() => TimeSpan.FromSeconds(Window);

        /// <summary>Cooldown'u TimeSpan olarak döndürür.</summary>
        public TimeSpan GetCooldown() => TimeSpan.FromSeconds(CooldownSeconds);
    }
}
