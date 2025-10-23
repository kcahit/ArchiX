// File: tests/ArchiX.WebApplication.Tests/Pipeline/PipelineOrderTests.cs
using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Behaviors;
using ArchiX.WebApplication.Pipeline;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Pipeline
{
    public sealed record OrderProbe(string? Name) : IRequest<string>;

    public sealed class OrderState { public bool ValidationRanBeforeHandler { get; set; } }

    public sealed class OrderProbeHandler : IRequestHandler<OrderProbe, string>
    {
        private readonly OrderState _state;
        public OrderProbeHandler(OrderState state) => _state = state;

        public Task<string> HandleAsync(OrderProbe request, CancellationToken cancellationToken)
        {
            Assert.True(_state.ValidationRanBeforeHandler);
            return Task.FromResult("OK");
        }
    }

    public sealed class OrderValidator : AbstractValidator<OrderProbe>
    {
        private readonly OrderState _state;
        public OrderValidator(OrderState state)
        {
            _state = state;
            RuleFor(x => x.Name).NotEmpty();
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<OrderProbe> context, CancellationToken cancellation = default)
        {
            _state.ValidationRanBeforeHandler = true;
            return base.ValidateAsync(context, cancellation);
        }
    }

    public sealed class PipelineOrderTests
    {
        private static ServiceProvider Build()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddSingleton<OrderState>();
            services.AddTransient<IRequestHandler<OrderProbe, string>, OrderProbeHandler>();
            services.AddTransient<IValidator<OrderProbe>, OrderValidator>();
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Validation_runs_before_handler()
        {
            var sp = Build();
            var mediator = sp.GetRequiredService<IMediator>();
            var res = await mediator.SendAsync(new OrderProbe("x"));
            Assert.Equal("OK", res);
        }
    }
}
