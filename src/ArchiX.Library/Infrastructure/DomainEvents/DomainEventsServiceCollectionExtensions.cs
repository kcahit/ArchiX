using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Infrastructure.DomainEvents
{
    /// <summary>
    /// Domain Events bileşenleri için Dependency Injection (DI) kayıt uzantıları.
    /// </summary>
    /// <remarks>
    /// <see cref="IEventDispatcher"/> implementasyonu varsayılan olarak
    /// <see cref="EventDispatcher"/> ile <c>Singleton</c> yaşam süresinde kaydedilir.
    /// </remarks>
    public static class DomainEventsServiceCollectionExtensions
    {
        /// <summary>
        /// Domain Events bileşenlerini DI konteynerine ekler.
        /// </summary>
        /// <param name="services">Servis koleksiyonu.</param>
        /// <returns>Aynı <paramref name="services"/> örneği (method chaining için).</returns>
        public static IServiceCollection AddArchiXDomainEvents(this IServiceCollection services)
        {
            services.AddSingleton<IEventDispatcher, EventDispatcher>();
            return services;
        }
    }
}
