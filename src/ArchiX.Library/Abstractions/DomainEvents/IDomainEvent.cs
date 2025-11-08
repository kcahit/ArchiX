namespace ArchiX.Library.Abstractions.DomainEvents;

/// <summary>Domain event sözleþmesi (UTC zaman damgasý içerir).</summary>
public interface IDomainEvent
{
 /// <summary>Olayýn gerçekleþtiði UTC zaman damgasý.</summary>
 DateTimeOffset OccurredOn { get; }
}
