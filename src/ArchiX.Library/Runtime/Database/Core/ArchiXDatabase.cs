// File: src/ArchiX.Library/Runtime/Database/Core/ArchiXDatabase.cs
namespace ArchiX.Library.Runtime.Database.Core
{
    /// <summary>
    /// Veritabanı kurulum ve güncelleme işlemleri için dış yüz.
    /// Sağlayıcı seçimini yapar ve ilgili provisioner’ı çalıştırır.
    /// </summary>
    public static class ArchiXDatabase
    {
        private const string DefaultProvider = "SqlServer";
        private static string _provider = DefaultProvider;

        // Sağlayıcı eş adları → kanonik ad
        private static readonly Dictionary<string, string> ProviderAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["SqlServer"] = "SqlServer",
            ["MSSQL"] = "SqlServer",
            ["Microsoft.SqlServer"] = "SqlServer"
        };

        /// <summary>Desteklenen sağlayıcı adları (kanonik).</summary>
        public static IReadOnlyCollection<string> SupportedProviders =>
            [.. ProviderAliases.Values.Distinct(StringComparer.OrdinalIgnoreCase)];

        /// <summary>
        /// Sağlayıcı yapılandırmasını değiştirir.
        /// Varsayılan: SqlServer. Desteklenmeyen sağlayıcı için <see cref="NotSupportedException"/> atar.
        /// </summary>
        public static void Configure(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                _provider = DefaultProvider;
                return;
            }

            if (!ProviderAliases.TryGetValue(providerName, out var normalized))
                throw new NotSupportedException($"Unsupported DB provider: {providerName}");

            _provider = normalized;
        }

        /// <summary>
        /// Sağlayıcıyı doğrular ve ayarlar. Başarı durumunu döner, hata fırlatmaz.
        /// </summary>
        public static bool TryConfigure(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                _provider = DefaultProvider;
                return true;
            }

            if (ProviderAliases.TryGetValue(providerName, out var normalized))
            {
                _provider = normalized;
                return true;
            }

            return false;
        }

        /// <summary>Yeni veritabanı oluşturur ve seed uygular.</summary>
        public static Task CreateAsync(CancellationToken ct = default) => GetProvisioner().CreateAsync(ct);

        /// <summary>Mevcut veritabanını günceller, bekleyen migration ve seed işlemlerini uygular.</summary>
        public static Task UpdateAsync(CancellationToken ct = default) => GetProvisioner().UpdateAsync(ct);

        private static ArchiXDbProvisionerBase GetProvisioner()
        {
            var provider = _provider ?? Environment.GetEnvironmentVariable("ARCHIX_DB_PROVIDER") ?? DefaultProvider;

            return provider.ToLowerInvariant() switch
            {
                "sqlserver" => new Providers.SqlServerArchiXDbProvisioner(),
                _ => throw new NotSupportedException($"Unsupported DB provider: {provider}")
            };
        }
    }
}
