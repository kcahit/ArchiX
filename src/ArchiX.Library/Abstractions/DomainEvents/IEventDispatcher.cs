namespace ArchiX.Library.Abstractions.DomainEvents;

/// <summary>
/// Domain event'leri ilgili handler'lara yayýnlar.
/// </summary>
public interface IEventDispatcher
{
 Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
