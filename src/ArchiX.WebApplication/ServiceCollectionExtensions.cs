using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication
{
 public static class ServiceCollectionExtensions
 {
 /// <summary>
 /// Register ArchiX recommended services for web application projects.
 /// Adds in-memory caching and repository caching decorator.
 /// </summary>
 public static IServiceCollection AddArchiXWebAppDefaults(this IServiceCollection services)
 {
 // Forward to the shared library implementation to avoid recursion
 return ArchiX.Library.Web.ServiceCollectionExtensions.AddArchiXWebDefaults(services);
 }
 }
}
