namespace ArchiX.Library.DomainEvents.Contracts
{
    /// <summary>
    /// Domain event işleyicisi sözleşmesi.
    /// </summary>
    /// <typeparam name="TEvent">
    /// İşlenecek domain event türü. <see cref="IDomainEvent"/> uygulamalıdır.
    /// </typeparam>
    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// Verilen domain event'ini işler.
        /// </summary>
        /// <param name="event">İşlenecek domain event örneği.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Asenkron işlem nesnesi.</returns>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
