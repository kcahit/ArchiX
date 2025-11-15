using Microsoft.Extensions.DependencyInjection;
using ArchiX.Library.Abstractions.DomainEvents;

namespace ArchiX.Library.Infrastructure.DomainEvents
{
    /// <summary>
    /// Domain Events bileşenleri için Dependency Injection (DI) kayıt uzantıları.
    /// </summary>
    /// <remarks>
    /// <see cref="IEventDispatcher"/> implementasyonu varsayılan olarak
    /// <see cref="EventDispatcher"/> ile <c>Singleton</c> yaşam süresinde kaydedilir.
    /// Ayrıca, yalnızca eski sözleşme ile kaydedilmiş handler'ların
    /// Abstractions arayüzü üzerinden de çözümlenebilmesi için bir köprü adaptör eklenir.
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
            // Register concrete dispatcher and map both abstraction and infra interfaces to same instance
            services.AddSingleton<EventDispatcher>();
            services.AddSingleton<ArchiX.Library.Abstractions.DomainEvents.IEventDispatcher>(sp => sp.GetRequiredService<EventDispatcher>());

            return services;
        }
    }
}
