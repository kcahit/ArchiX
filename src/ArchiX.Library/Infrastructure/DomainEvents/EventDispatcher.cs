using ArchiX.Library.Abstractions.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Infrastructure.DomainEvents
{
    /// <summary>
    /// DI içindeki IEventHandler implementasyonlarını (hem Abstractions hem de legacy Contracts) bularak
    /// domain event'leri sırasıyla çalıştıran yayınlayıcı.
    /// </summary>
    public sealed class EventDispatcher(IServiceProvider serviceProvider) : ArchiX.Library.Abstractions.DomainEvents.IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        /// <inheritdoc />
        public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            if (events is null) return;
            using var scope = _serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            foreach (var @event in events)
            {
                if (@event is null) continue;
                var eventType = @event.GetType();

                var absHandlerInterface = typeof(ArchiX.Library.Abstractions.DomainEvents.IEventHandler<>).MakeGenericType(eventType);
                var handlers = provider.GetServices(absHandlerInterface).ToArray();

                foreach (var handler in handlers)
                {
                    // Invoke HandleAsync via reflection on the strongly-typed interface to avoid dynamic binder issues
                    var method = absHandlerInterface.GetMethod("HandleAsync");
                    var task = (Task?)method?.Invoke(handler, [@event, cancellationToken]);
                    if (task != null) await task.ConfigureAwait(false);
                }
            }
        }
    }
}
