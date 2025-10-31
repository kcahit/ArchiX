using System.Reflection;

using ArchiX.WebApplication.Abstractions.Interfaces;
using ArchiX.WebApplication.Behaviors;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Pipeline
{
    /// <summary>
    /// DI kayıtları için yardımcı uzantılar.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Belirtilen derlemedeki <see cref="IRequestHandler{TRequest,TResponse}"/> türlerini kaydeder.
        /// </summary>
        public static IServiceCollection AddArchiXHandlersFrom(this IServiceCollection services, Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));

            var handlerOpenType = typeof(IRequestHandler<,>);
            var types = assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Select(t => new
                {
                    Impl = t,
                    Service = t
                        .GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerOpenType)
                })
                .Where(x => x.Service != null);

            foreach (var t in types)
            {
                var exists = services.Any(sd =>
                    sd.ServiceType == t.Service &&
                    sd.ImplementationType == t.Impl);

                if (!exists)
                {
                    services.AddTransient(t.Service!, t.Impl);
                }
            }

            return services;
        }

        /// <summary>
        /// Authorization davranışını pipeline'a ekler.
        /// </summary>
        public static IServiceCollection AddArchiXAuthorizationPipeline(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            return services;
        }

        /// <summary>
        /// Validation davranışını pipeline'a ekler.
        /// </summary>
        public static IServiceCollection AddArchiXValidationPipeline(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            return services;
        }

        /// <summary>
        /// Transaction davranışını pipeline'a ekler.
        /// </summary>
        public static IServiceCollection AddArchiXTransactionPipeline(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            return services;
        }
    }
}
