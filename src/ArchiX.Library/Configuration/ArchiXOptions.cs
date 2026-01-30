using System;
using System.Collections.Generic;

namespace ArchiX.Library.Configuration
{
    /// <summary>
    /// ArchiX çekirdek ayarları (connection string, cache süreleri, host mapping).
    /// </summary>
    public sealed class ArchiXOptions
    {
        /// <summary>ArchiX veritabanı bağlantı dizesi.</summary>
        public string ArchiXConnectionString { get; set; } = string.Empty;

        /// <summary>ArchiX DbContext migration assembly adı (opsiyonel).</summary>
		public string? ArchiXMigrationsAssembly { get; set; }
        /// <summary>Varsayılan ApplicationId (fallback).</summary>
        public int DefaultApplicationId { get; set; } = 1;

        /// <summary>Host → ApplicationId eşlemeleri.</summary>
        public Dictionary<string, int> HostApplicationMapping { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Menü cache süresi (varsayılan: 1 saat).</summary>
        public TimeSpan MenuCacheDuration { get; set; } = TimeSpan.FromHours(1);

        /// <summary>Parametre cache süresi (varsayılan: 30 dakika).</summary>
        public TimeSpan ParameterCacheDuration { get; set; } = TimeSpan.FromMinutes(30);
    }
}
