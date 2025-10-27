// File: tests/ArchiX.WebApplication.Tests/Pipeline/CqrsRegistrationExtensionsTests.cs
using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Behaviors;
using ArchiX.WebApplication.Pipeline;
using ArchiX.WebApplication.Tests.Behaviors.TransactionBehavior; // FakeUnitOfWork

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Pipeline
{
    public sealed record Boot2Req(string V) : IRequest<string>;

    public sealed class Boot2Handler : IRequestHandler<Boot2Req, string>
    {
        public Task<string> HandleAsync(Boot2Req r, CancellationToken ct) => Task.FromResult(r.V);
    }

    public sealed class CqrsRegistrationExtensionsTests
    {
        [Fact]
        public async Task AddArchiXCqrs_Composes_And_Resolves_Mediator()
        {
            var s = new ServiceCollection();
            s.AddSingleton<IUnitOfWork, FakeUnitOfWork>();                 // <-- eklendi
            s.AddArchiXCqrs(typeof(CqrsRegistrationExtensionsTests).Assembly);
            s.AddArchiXHandlersFrom(typeof(CqrsRegistrationExtensionsTests).Assembly);
            var sp = s.BuildServiceProvider();

            var m1 = sp.GetRequiredService<IMediator>();
            var m2 = sp.GetRequiredService<IMediator>();
            Assert.Same(m1, m2);

            var res = await m1.SendAsync(new Boot2Req("ok"));
            Assert.Equal("ok", res);
        }

        [Fact]
        public void Registers_Validation_then_Transaction_behaviors()
        {
            var s = new ServiceCollection();
            s.AddArchiXCqrs();

            var descriptors = s
                .Where(sd => sd.ServiceType.IsGenericType &&
                             sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                .ToArray();

            Assert.True(descriptors.Length >= 2);
            Assert.Equal(typeof(ValidationBehavior<,>), descriptors[0].ImplementationType);
            Assert.Equal(typeof(TransactionBehavior<,>), descriptors[1].ImplementationType);
        }
    }
}
