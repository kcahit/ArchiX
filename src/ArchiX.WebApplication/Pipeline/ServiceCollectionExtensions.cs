// File: ArchiX.WebApplication/Pipeline/ServiceCollectionExtensions.cs
using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Behaviors;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Pipeline
{
    /// <summary>
    /// DI kayıt uzantıları.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// IMediator kaydını ekler.
        /// </summary>
        public static IServiceCollection AddArchiXMediator(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<IMediator, Mediator>();
            return services;
        }

        /// <summary>
        /// Validation pipeline davranışını ekler.
        /// </summary>
        public static IServiceCollection AddArchiXValidationPipeline(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            return services;
        }
    }
}
