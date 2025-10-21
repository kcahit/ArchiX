// File: tests/ArchiX.WebApplication.Tests/Pipeline/CqrsRegistrationExtensionsTests.cs
using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Pipeline;

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
            s.AddArchiXCqrs(typeof(CqrsRegistrationExtensionsTests).Assembly);
            s.AddArchiXHandlersFrom(typeof(CqrsRegistrationExtensionsTests).Assembly);
            var sp = s.BuildServiceProvider();

            var m1 = sp.GetRequiredService<IMediator>();
            var m2 = sp.GetRequiredService<IMediator>();
            Assert.Same(m1, m2); // singleton olduğu doğrulansın

            var res = await m1.SendAsync(new Boot2Req("ok"));
            Assert.Equal("ok", res);
        }
    }
}
