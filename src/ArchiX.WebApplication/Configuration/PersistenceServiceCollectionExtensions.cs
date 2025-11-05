using ArchiX.Library.Abstractions.Persistence;
using ArchiX.Library.Infrastructure.EfCore;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Configuration
{
    /// <summary>
    /// Persistence katmanı DI kayıtları.
    /// </summary>
    public static class PersistenceServiceCollectionExtensions
    {
        /// <summary>
        /// IUnitOfWork → UnitOfWork (scoped) kaydını ekler.
        /// </summary>
        public static IServiceCollection AddArchiXPersistence(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
