using ArchiX.Library.DomainEvents.Contracts;
using ArchiX.Library.Infrastructure.DomainEvents;

using Xunit;

namespace ArchiX.Library.Tests.Tests.DomainEventsTests
{
    /// <summary>Dispatcher'ın kayıtlı handler'ları çağırdığını doğrular.</summary>
    public class EventDispatcherTests
    {
        // Basit test event'i
        private sealed class TestEvent : DomainEvent { }

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
            services.AddArchiXDomainEvents(); // IEventDispatcher -> EventDispatcher
            services.AddScoped<IEventHandler<TestEvent>, TestEventHandler>();

            using var provider = services.BuildServiceProvider();
            var dispatcher = provider.GetRequiredService<IEventDispatcher>();

            // Act
            TestEventHandler.Count = 0;
            await dispatcher.DispatchAsync([new TestEvent()]); // C# 12 collection expression

            // Assert
            Assert.Equal(1, TestEventHandler.Count);
        }
    }
}
