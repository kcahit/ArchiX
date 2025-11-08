using ArchiX.Library.Web.Abstractions.Delegates;
using ArchiX.Library.Web.Abstractions.Interfaces;
using ArchiX.Library.Web.Behaviors;
using ArchiX.Library.Web.Pipeline;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArchiX.Library.Web.Tests.Pipeline
{
 public sealed record Order2Req(string Name) : IRequest<string>;
 public sealed class Trace { public List<string> Steps { get; } = new(); }
 public sealed class Order2Handler : IRequestHandler<Order2Req, string>
 {
 private readonly Trace _trace;
 public Order2Handler(Trace trace) => _trace = trace;
 public Task<string> HandleAsync(Order2Req req, CancellationToken ct)
 {
 _trace.Steps.Add("Handler");
 return Task.FromResult("OK");
 }
 }

 public sealed class Order2Validator : AbstractValidator<Order2Req>
 {
 private readonly Trace _trace;
 public Order2Validator(Trace trace)
 {
 _trace = trace;
 RuleFor(x => x.Name).NotEmpty();
 }
 public override Task<FluentValidation.Results.ValidationResult> ValidateAsync(FluentValidation.ValidationContext<Order2Req> context, CancellationToken ct = default)
 {
 _trace.Steps.Add("Validation");
 return base.ValidateAsync(context, ct);
 }
 }

 public sealed class BehaviorA<TReq, TRes> : IPipelineBehavior<TReq, TRes> where TReq : IRequest<TRes>
 {
 private readonly Trace _trace;
 public BehaviorA(Trace trace) => _trace = trace;
 public async Task<TRes> HandleAsync(TReq req, RequestHandlerDelegate<TRes> next, CancellationToken ct)
 {
 _trace.Steps.Add("A");
 return await next(ct);
 }
 }

 public sealed class BehaviorB<TReq, TRes> : IPipelineBehavior<TReq, TRes> where TReq : IRequest<TRes>
 {
 private readonly Trace _trace;
 public BehaviorB(Trace trace) => _trace = trace;
 public async Task<TRes> HandleAsync(TReq req, RequestHandlerDelegate<TRes> next, CancellationToken ct)
 {
 _trace.Steps.Add("B");
 return await next(ct);
 }
 }

 public sealed class PipelineMultiBehaviorOrderTests
 {
 private static readonly string[] expected = new[] { "A", "Validation", "B", "Handler" };

 [Fact]
 public async Task Order_Is_A_Then_Validation_Then_B_Then_Handler()
 {
 var s = new ServiceCollection();
 s.AddSingleton<IMediator, Mediator>();
 s.AddSingleton<Trace>();

 s.AddSingleton(typeof(IPipelineBehavior<,>), typeof(BehaviorA<,>));
 s.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
 s.AddSingleton(typeof(IPipelineBehavior<,>), typeof(BehaviorB<,>));

 s.AddTransient<IRequestHandler<Order2Req, string>, Order2Handler>();
 s.AddTransient<IValidator<Order2Req>, Order2Validator>();

 var sp = s.BuildServiceProvider();
 var med = sp.GetRequiredService<IMediator>();

 var result = await med.SendAsync(new Order2Req("ok"));
 var trace = sp.GetRequiredService<Trace>();

 Assert.Equal("OK", result);
 Assert.Equal(expected, trace.Steps);
 }
 }
}
