using ArchiX.Library.Runtime.Database.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Database
{
    /// <summary>
    /// Kütüphaneyi projeye ekleyen müþteri geliþtiricinin tek satýrla
    /// veritabaný oluþturma / migrate / seed iþlemini tetiklemesini saðlar.
    /// - DB adý sabittir ("ArchiX").
    /// - Admin þifresi MUST be provided via env var: ARCHIX_DB_ADMIN_PASSWORD.
    /// - Çalýþmasý için <c>ArchiX:AllowDbOps</c> konfigürasyonu true olmalý veya <c>force=true</c> verilmeli.
    /// </summary>
    public static class AdminProvisionerRunner
    {
        public static async Task EnsureDatabaseProvisionedAsync(IServiceProvider services, bool force = false, CancellationToken ct = default)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var cfg = services.GetService<IConfiguration>();
            var logger = services.GetService<ILoggerFactory>()?.CreateLogger("ArchiX.Database.Provisioner");

            var allow = cfg?.GetValue<bool>("ArchiX:AllowDbOps", false) ?? false;
            if (!allow && !force)
            {
                logger?.LogInformation("[ArchiX] DB ops skipped because ArchiX:AllowDbOps is false and force is not set.");
                return;
            }

            try
            {
                // Saðlayýcý sabit: SqlServer (mevcut implementation destekliyor)
                ArchiXDatabase.Configure("SqlServer");

                logger?.LogInformation("[ArchiX] Starting DB provisioner (CreateAsync).");
                await ArchiXDatabase.CreateAsync(ct).ConfigureAwait(false);
                logger?.LogInformation("[ArchiX] DB provisioner finished successfully.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[ArchiX] DB provisioner failed.");
                throw;
            }
        }
    }
}
