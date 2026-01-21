using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Runtime.Security;
using ArchiX.Library.Web.Security.Authorization;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// ArchiX web varsayılanları: bellek önbelleği, repo önbelleği,
        /// parola güvenliği (policy provider + Argon2 hasher) ve yetki policy kayıtları.
        /// </summary>
        public static IServiceCollection AddArchiXWebDefaults(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddArchiXMemoryCaching();
            services.AddArchiXRepositoryCaching();

            // Parola politikası sağlayıcısı + Argon2 hasher
            services.AddPasswordSecurity();

            // Yetkilendirme policy kayıtları (Admin/User/CanExport/CanImport)
            services.AddArchiXPolicies();

            services.AddScoped<ArchiX.Library.Web.Abstractions.Reports.IReportDatasetOptionService, ArchiX.Library.Web.Runtime.Reports.ReportDatasetOptionService>();

            // UI timeout options (şimdilik hard-coded, sonra DB'den gelecek)
            services.Configure<UiTimeoutOptions>(opts => { });

            return services;
        }
    }
}
