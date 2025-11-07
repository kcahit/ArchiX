using ArchiX.Library.Abstractions.Persistence;
using ArchiX.Library.Infrastructure.EfCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Configuration
{
 public static class PersistenceServiceCollectionExtensions
 {
 public static IServiceCollection AddArchiXPersistence(this IServiceCollection services)
 {
 services.AddScoped<IUnitOfWork, UnitOfWork>();
 return services;
 }
 }
}
