// File: tests/ArchiX.WebApplication.Tests/Pipeline/MediatorTests.cs
using System.Reflection;

using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Pipeline
{
    public sealed record MReq(string Msg) : IRequest<string>;

    public sealed class MHandler : IRequestHandler<MReq, string>
    {
        public Task<string> HandleAsync(MReq request, CancellationToken ct) =>
            Task.FromResult($"[{request.Msg}]");
    }

    public sealed class MHandler2 : IRequestHandler<MReq, string>
    {
        public Task<string> HandleAsync(MReq request, CancellationToken ct) =>
            Task.FromResult($"2[{request.Msg}]");
    }

    public sealed class MediatorTests
    {
        private static ServiceProvider BaseSvc(Action<IServiceCollection>? cfg = null)
        {
            var s = new ServiceCollection();
            s.AddSingleton<IMediator, Mediator>();
            cfg?.Invoke(s);
            return s.BuildServiceProvider();
        }

        private static Exception Unwrap(Exception e) =>
            e is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : e;

        [Fact]
        public async Task Routes_To_Single_Handler()
        {
            var sp = BaseSvc(s => s.AddTransient<IRequestHandler<MReq, string>, MHandler>());
            var med = sp.GetRequiredService<IMediator>();
            var res = await med.SendAsync(new MReq("OK"));
            Assert.Equal("[OK]", res);
        }

        [Fact]
        public async Task Throws_When_No_Handler()
        {
            var sp = BaseSvc();
            var med = sp.GetRequiredService<IMediator>();
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => med.SendAsync(new MReq("X")));
            Assert.IsType<InvalidOperationException>(Unwrap(ex));
            Assert.Contains("No handler", Unwrap(ex).Message);
        }

        [Fact]
        public async Task Throws_When_Multiple_Handlers()
        {
            var sp = BaseSvc(s =>
            {
                s.AddTransient<IRequestHandler<MReq, string>, MHandler>();
                s.AddTransient<IRequestHandler<MReq, string>, MHandler2>();
            });

            // Tanılama: gerçekten 2 handler kayıtlı mı?
            var all = sp.GetServices<IRequestHandler<MReq, string>>().ToList();
            Assert.Equal(2, all.Count);

            var med = sp.GetRequiredService<IMediator>();
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => med.SendAsync(new MReq("X")));
            Assert.IsType<InvalidOperationException>(Unwrap(ex));
            Assert.Contains("Multiple handlers", Unwrap(ex).Message);
        }

        [Fact]
        public async Task Flows_CancellationToken()
        {
            var seen = false;
            var sp = BaseSvc(s =>
            {
                s.AddTransient<IRequestHandler<MReq, string>>(_ =>
                    new InlineHandler((_, ct) =>
                    {
                        seen = ct.IsCancellationRequested;
                        ct.ThrowIfCancellationRequested();
                        return Task.FromResult("n/a");
                    }));
            });

            var med = sp.GetRequiredService<IMediator>();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var ex = await Assert.ThrowsAnyAsync<Exception>(() => med.SendAsync(new MReq("X"), cts.Token));
            Assert.IsType<OperationCanceledException>(Unwrap(ex));
            Assert.True(seen);
        }

        private sealed class InlineHandler : IRequestHandler<MReq, string>
        {
            private readonly Func<MReq, CancellationToken, Task<string>> _f;
            public InlineHandler(Func<MReq, CancellationToken, Task<string>> f) => _f = f;
            public Task<string> HandleAsync(MReq r, CancellationToken ct) => _f(r, ct);
        }
    }
}
