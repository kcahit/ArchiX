using ArchiX.Library.DomainEvents.Contracts;

namespace ArchiX.Library.Infrastructure.Others
{
    /// <summary>
    /// Toplanan domain event'leri ilgili <see cref="IEventHandler{TEvent}"/> uygulamalarına
    /// yayınlamaktan sorumlu arayüz.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Verilen domain event koleksiyonunu asenkron olarak yayınlar.
        /// </summary>
        /// <param name="events">Yayınlanacak domain event listesi.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    }
}
