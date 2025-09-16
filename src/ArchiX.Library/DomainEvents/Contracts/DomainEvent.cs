namespace ArchiX.Library.DomainEvents.Contracts
{
    /// <summary>
    /// Domain event'ler için ortak taban sınıf.
    /// </summary>
    /// <remarks>
    /// Tüm event'ler için UTC zaman damgası taşır. Türetilen event'ler, oluştuğu anda
    /// <see cref="OccurredOn"/> değerini otomatik alır.
    /// </remarks>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Olayın gerçekleştiği UTC zaman damgası.
        /// </summary>
        public DateTimeOffset OccurredOn { get; }

        /// <summary>
        /// Varsayılan kurucu; <see cref="OccurredOn"/> alanını şu anın UTC değerine ayarlar.
        /// </summary>
        protected DomainEvent() : this(DateTimeOffset.UtcNow) { }

        /// <summary>
        /// İsteğe bağlı özel zaman damgası ile kurucu.
        /// </summary>
        /// <param name="occurredOn">Olayın gerçekleştiği UTC zaman damgası.</param>
        protected DomainEvent(DateTimeOffset occurredOn) => OccurredOn = occurredOn;
    }
}
