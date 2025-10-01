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

        /// <summary>
        /// Sağlayıcı yapılandırmasını değiştirir.
        /// Varsayılan: SqlServer.
        /// </summary>
        public static void Configure(string providerName)
        {
            _provider = string.IsNullOrWhiteSpace(providerName) ? DefaultProvider : providerName;
        }

        /// <summary>
        /// Yeni veritabanı oluşturur ve seed uygular.
        /// </summary>
        public static Task CreateAsync(CancellationToken ct = default)
        {
            return GetProvisioner().CreateAsync(ct);
        }

        /// <summary>
        /// Mevcut veritabanını günceller, bekleyen migration ve seed işlemlerini uygular.
        /// </summary>
        public static Task UpdateAsync(CancellationToken ct = default)
        {
            return GetProvisioner().UpdateAsync(ct);
        }

        private static ArchiXDbProvisionerBase GetProvisioner()
        {
            var provider = _provider ?? Environment.GetEnvironmentVariable("ARCHIX_DB_PROVIDER") ?? DefaultProvider;

            return provider switch
            {
                "SqlServer" => new Providers.SqlServerArchiXDbProvisioner(),
                //"Oracle" => new Providers.OracleArchiXDbProvisioner(), // örnek olarak azıldı. ileride olabilir diye bıraktım.
                _ => throw new NotSupportedException($"Unsupported DB provider: {provider}")
            };
        }
    }
}
