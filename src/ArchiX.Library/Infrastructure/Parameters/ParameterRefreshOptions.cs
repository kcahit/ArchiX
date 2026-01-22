namespace ArchiX.Library.Infrastructure.Parameters
{
    /// <summary>#57 Parametre cache TTL süreleri (DB-driven).</summary>
    public sealed class ParameterRefreshOptions
    {
        /// <summary>UI parametreleri cache TTL (saniye). Varsayılan: 300 (5 dakika).</summary>
        public int UiCacheTtlSeconds { get; set; } = 300;

        /// <summary>HTTP parametreleri cache TTL (saniye). Varsayılan: 60 (1 dakika).</summary>
        public int HttpCacheTtlSeconds { get; set; } = 60;

        /// <summary>Security parametreleri cache TTL (saniye). Varsayılan: 30.</summary>
        public int SecurityCacheTtlSeconds { get; set; } = 30;

        /// <summary>UI cache TTL'yi TimeSpan olarak döndürür.</summary>
        public TimeSpan GetUiCacheTtl() => TimeSpan.FromSeconds(UiCacheTtlSeconds);

        /// <summary>HTTP cache TTL'yi TimeSpan olarak döndürür.</summary>
        public TimeSpan GetHttpCacheTtl() => TimeSpan.FromSeconds(HttpCacheTtlSeconds);

        /// <summary>Security cache TTL'yi TimeSpan olarak döndürür.</summary>
        public TimeSpan GetSecurityCacheTtl() => TimeSpan.FromSeconds(SecurityCacheTtlSeconds);
    }
}
