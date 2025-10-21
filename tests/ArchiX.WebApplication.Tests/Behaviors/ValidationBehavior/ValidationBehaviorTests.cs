// File: tests/ArchiX.WebApplication.Tests/Behaviors/ValidationBehavior/ValidationBehaviorTests.cs
using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Pipeline;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Behaviors.ValidationBehavior
{
    /// <summary>
    /// 7,0200 — ValidationBehavior entegrasyon testleri.
    /// </summary>
    public sealed class ValidationBehaviorTests
    {
        [Fact]
        public async Task Valid_request_returns_handler_result()
        {
            var services = new ServiceCollection();
            services.AddArchiXCqrs(typeof(ValidationBehaviorTests).Assembly);
            services.AddArchiXHandlersFrom(typeof(ValidationBehaviorTests).Assembly);
            var sp = services.BuildServiceProvider();

            var mediator = sp.GetRequiredService<IMediator>();
            var result = await mediator.SendAsync(new EchoRequest { Name = "Cahit" });

            Assert.Equal("Hello, Cahit!", result);
        }

        [Fact]
        public async Task Invalid_request_throws_ValidationException()
        {
            var services = new ServiceCollection();
            services.AddArchiXCqrs(typeof(ValidationBehaviorTests).Assembly);
            services.AddArchiXHandlersFrom(typeof(ValidationBehaviorTests).Assembly);
            var sp = services.BuildServiceProvider();

            var mediator = sp.GetRequiredService<IMediator>();

            await Assert.ThrowsAsync<ValidationException>(() =>
                mediator.SendAsync(new EchoRequest { Name = "" }));
        }
    }
}
