using ArchiX.Library.DomainEvents.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Infrastructure.DomainEvents
{
    /// <summary>
    /// DI içindeki <see cref="IEventHandler{TEvent}"/> uygulamalarını bularak
    /// domain event'leri sırasıyla çalıştıran yayınlayıcı.
    /// </summary>
    public sealed class EventDispatcher(IServiceProvider serviceProvider) : IEventDispatcher
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

                var handlerInterface = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
                var handlersEnumerableType = typeof(IEnumerable<>).MakeGenericType(handlerInterface);

                if (provider.GetService(handlersEnumerableType) is not System.Collections.IEnumerable handlers)
                    continue;

                var handleMethod = handlerInterface.GetMethod(
                    nameof(IEventHandler<IDomainEvent>.HandleAsync)
                );
                if (handleMethod is null) continue;

                foreach (var handler in handlers)
                {
                    var task = (Task?)handleMethod.Invoke(handler, [@event, cancellationToken]);
                    if (task is not null)
                        await task.ConfigureAwait(false);
                }
            }
        }
    }
}
