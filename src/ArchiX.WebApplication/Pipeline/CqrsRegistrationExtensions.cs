using ArchiX.WebApplication.Abstractions.Interfaces;
using ArchiX.WebApplication.Behaviors;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Pipeline
{
    /// <summary>
    /// CQRS bileşenleri için DI kayıtları.
    /// </summary>
    public static class CqrsRegistrationExtensions
    {
        /// <summary>
        /// ArchiX CQRS kayıtlarını ekler.
        /// Pipeline sırası: Authorization → Validation → Transaction.
        /// IMediator tekil (singleton) kaydedilir.
        /// </summary>
        public static IServiceCollection AddArchiXCqrs(this IServiceCollection services)
        {
            services.AddSingleton<IMediator, Mediator>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            return services;
        }

        /// <summary>
        /// Belirtilen derleme bağlamında CQRS kayıtlarını ekler.
        /// Not: İstek işleyicileri bu metotta taranmaz; testlerde ayrıca AddArchiXHandlersFrom(assembly) çağrılır.
        /// </summary>
        public static IServiceCollection AddArchiXCqrs(this IServiceCollection services, System.Reflection.Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            return services.AddArchiXCqrs();
        }
    }
}
