using Microsoft.Extensions.DependencyInjection;
using ArchiX.Library.Infrastructure.Caching;

namespace ArchiX.Library.Web
{
 public static class ServiceCollectionExtensions
 {
 public static IServiceCollection AddArchiXWebDefaults(this IServiceCollection services)
 {
 ArgumentNullException.ThrowIfNull(services);
 services.AddArchiXMemoryCaching();
 services.AddArchiXRepositoryCaching();
 return services;
 }
 }
}
