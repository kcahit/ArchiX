// File: src/ArchiX.WebApplication/Pipeline/CqrsRegistrationExtensions.cs
using System.Reflection;
using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Pipeline
{
    /// <summary>
    /// CQRS ve pipeline davranışlarının DI kayıtları.
    /// </summary>
    public static class CqrsRegistrationExtensions
    {
        /// <summary>
        /// IMediator ve pipeline davranışlarını ekler; verilen assembly’lerden validator’ları tarar.
        /// Kayıt sırası → dıştan içe: Validation → Transaction → Handler.
        /// </summary>
        public static IServiceCollection AddArchiXCqrs(this IServiceCollection services, params Assembly[] scanAssemblies)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<IMediator, Mediator>();

            // Kayıt sırası önemlidir: GetServices().Reverse() ile dış sarmal oluyor.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            if (scanAssemblies is { Length: > 0 })
            {
                ValidationServiceCollectionExtensions.AddArchiXValidatorsFrom(services, scanAssemblies);
            }

            return services;
        }
    }
}
