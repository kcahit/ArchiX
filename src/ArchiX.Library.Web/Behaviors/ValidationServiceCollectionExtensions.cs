using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;

namespace ArchiX.Library.Web.Behaviors
{
 public static class ValidationServiceCollectionExtensions
 {
 public static IServiceCollection AddArchiXValidatorsFrom(this IServiceCollection services, params Assembly[] assemblies)
 {
 ArgumentNullException.ThrowIfNull(services);
 if (assemblies is null || assemblies.Length ==0) return services;
 foreach (var asm in assemblies)
 {
 Type[] types;
 try { types = asm.GetExportedTypes(); }
 catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null)!.ToArray()!; }
 foreach (var type in types)
 {
 if (!type.IsClass || type.IsAbstract) continue;
 var validatorIfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
 foreach (var iface in validatorIfaces) services.AddTransient(iface, type);
 }
 }
 return services;
 }
 }
}
