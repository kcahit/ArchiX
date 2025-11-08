namespace ArchiX.Library.Abstractions.DomainEvents;

/// <summary>Domain event iþleyicisi sözleþmesi.</summary>
/// <typeparam name="TEvent">Ýþlenecek event; <see cref="IDomainEvent"/> uygulamalýdýr.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
 /// <summary>Event'i iþler.</summary>
 Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
