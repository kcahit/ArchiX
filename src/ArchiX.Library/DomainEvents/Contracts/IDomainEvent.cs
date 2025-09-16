

namespace ArchiX.Library.DomainEvents.Contracts
{
    /// <summary>Domain event sözleşmesi.</summary>
    public interface IDomainEvent
    {
        /// <summary>Olayın gerçekleştiği UTC zaman damgası.</summary>
        /// <remarks>Handler'lar arasında tutarlılık için değer UTC olarak tutulur.</remarks>
        DateTimeOffset OccurredOn { get; }
    }
}
