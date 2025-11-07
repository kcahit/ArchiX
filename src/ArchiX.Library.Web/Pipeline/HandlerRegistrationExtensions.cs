using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Pipeline
{
 public static class HandlerRegistrationExtensions
 {
 public static IServiceCollection AddArchiXHandlersFrom(this IServiceCollection services, params Assembly[] assemblies)
 {
 ArgumentNullException.ThrowIfNull(services);
 if (assemblies is null || assemblies.Length ==0) return services;
 foreach (var asm in assemblies)
 {
 Type[] types;
 try { types = asm.GetExportedTypes(); }
 catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null)!.ToArray()!; }
 foreach (var impl in types.Where(t => t is { IsClass: true, IsAbstract: false }))
 {
 foreach (var iface in impl.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ArchiX.Library.Web.Abstractions.Interfaces.IRequestHandler<,>)))
 {
 services.AddTransient(iface, impl);
 }
 }
 }
 return services;
 }
 }
}
