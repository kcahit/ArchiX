using ArchiX.Library.Abstractions.DomainEvents;
using ArchiX.Library.Infrastructure.DomainEvents; // Extension method için
using IEventDispatcher = ArchiX.Library.Abstractions.DomainEvents.IEventDispatcher;

using Xunit;

namespace ArchiX.Library.Tests.Tests.DomainEventsTests
{
    /// <summary>Dispatcher'ın kayıtlı handler'ları çağırdığını doğrular.</summary>
    public class EventDispatcherTests
    {
        // Basit test event'i
        private sealed class TestEvent : IDomainEvent
        {
            public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
        }

        // Sayaçlı handler
        private sealed class TestEventHandler : IEventHandler<TestEvent>
        {
            public static int Count; // set edilebilir alan
            public Task HandleAsync(TestEvent @event, CancellationToken _ = default)
            {
                Interlocked.Increment(ref Count);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task DispatchAsync_calls_registered_handlers()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddArchiXDomainEvents();
            services.AddScoped<IEventHandler<TestEvent>, TestEventHandler>();
            using var provider = services.BuildServiceProvider();
            var dispatcher = provider.GetRequiredService<IEventDispatcher>();

            // Act
            TestEventHandler.Count =0;
#pragma warning disable IDE0300
            await dispatcher.DispatchAsync(new[] { new TestEvent() });
#pragma warning restore IDE0300

            // Assert
            Assert.Equal(1, TestEventHandler.Count);
        }
    }
}
