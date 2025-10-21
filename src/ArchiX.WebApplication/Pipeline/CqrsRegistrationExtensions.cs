// File: ArchiX.WebApplication/Pipeline/CqrsRegistrationExtensions.cs
using System.Reflection;

using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Behaviors;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Pipeline
{
    /// <summary>
    /// CQRS ve doğrulama hattı DI kayıtları için kısayollar.
    /// </summary>
    public static class CqrsRegistrationExtensions
    {
        /// <summary>
        /// IMediator, ValidationBehavior ve (varsa) verilen assembly’lerdeki tüm IValidator&lt;T&gt; türlerini kaydeder.
        /// </summary>
        /// <param name="services">DI koleksiyonu.</param>
        /// <param name="scanAssemblies">IValidator taraması yapılacak assembly listesi (opsiyonel).</param>
        public static IServiceCollection AddArchiXCqrs(this IServiceCollection services, params Assembly[] scanAssemblies)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<IMediator, Mediator>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            if (scanAssemblies is { Length: > 0 })
            {
                // ValidationServiceCollectionExtensions.AddArchiXValidatorsFrom
                ArchiX.WebApplication.Behaviors.ValidationServiceCollectionExtensions
                    .AddArchiXValidatorsFrom(services, scanAssemblies);
            }

            return services;
        }
    }
}
