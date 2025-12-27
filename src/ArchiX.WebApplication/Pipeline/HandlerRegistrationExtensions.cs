// File: ArchiX.WebApplication/Pipeline/HandlerRegistrationExtensions.cs
using System.Reflection;

using ArchiX.WebApplication.Abstractions.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Pipeline
{
    /// <summary>
    /// IRequestHandler&lt;TRequest,TResponse&gt; uygulamalarını yansıma ile DI’a ekler.
    /// </summary>
    public static class HandlerRegistrationExtensions
    {
        /// <summary>
        /// Verilen assembly’lerdeki tüm IRequestHandler&lt;,&gt; türlerini transient olarak kaydeder.
        /// </summary>
        public static IServiceCollection AddArchiXHandlersFrom(this IServiceCollection services, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            if (assemblies is null || assemblies.Length == 0) return services;

            foreach (var asm in assemblies)
            {
                Type[] types;
                try { types = asm.GetExportedTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null)!.ToArray()!; }

                foreach (var impl in types.Where(t => t is { IsClass: true, IsAbstract: false }))
                {
                    foreach (var iface in impl.GetInterfaces()
                             .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                    {
                        services.AddTransient(iface, impl);
                    }
                }
            }

            return services;
        }
    }
}
