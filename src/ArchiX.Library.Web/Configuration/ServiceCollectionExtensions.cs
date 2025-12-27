using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Runtime.Security;
using ArchiX.Library.Web.Security.Authorization;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// ArchiX web varsayýlanlarý: bellek önbelleði, repo önbelleði,
        /// parola güvenliði (policy provider + Argon2 hasher) ve yetki policy kayýtlarý.
        /// </summary>
        public static IServiceCollection AddArchiXWebDefaults(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddArchiXMemoryCaching();
            services.AddArchiXRepositoryCaching();

            // Parola politikasý saðlayýcýsý + Argon2 hasher
            services.AddPasswordSecurity();

            // Yetkilendirme policy kayýtlarý (Admin/User/CanExport/CanImport)
            services.AddArchiXPolicies();

            return services;
        }
    }
}
