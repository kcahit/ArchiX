// File: tests/ArchiX.WebApplication.Tests/Behaviors/ValidationBehavior/ValidationBehaviorTests.cs
using ArchiX.WebApplication.Abstractions.Interfaces;
using ArchiX.WebApplication.Behaviors;
using ArchiX.WebApplication.Pipeline;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Behaviors.ValidationBehavior
{
    public sealed record VbRequest(string? Name) : IRequest<string>;

    public sealed class VbHandler : IRequestHandler<VbRequest, string>
    {
        public Task<string> HandleAsync(VbRequest request, CancellationToken cancellationToken)
            => Task.FromResult($"Hello, {request.Name}!");
    }

    public sealed class VbValidator : AbstractValidator<VbRequest>
    {
        public VbValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Name required");
    }

    public sealed class ValidationBehaviorTests
    {
        private static ServiceProvider Build()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient<IRequestHandler<VbRequest, string>, VbHandler>();
            services.AddTransient<IValidator<VbRequest>, VbValidator>();
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Valid_request_returns_handler_result()
        {
            var sp = Build();
            var mediator = sp.GetRequiredService<IMediator>();
            var result = await mediator.SendAsync(new VbRequest("Cahit"));
            Assert.Equal("Hello, Cahit!", result);
        }

        [Fact]
        public async Task Invalid_request_throws_ValidationException()
        {
            var sp = Build();
            var mediator = sp.GetRequiredService<IMediator>();
            await Assert.ThrowsAsync<ValidationException>(() => mediator.SendAsync(new VbRequest("")));
        }
    }
}
