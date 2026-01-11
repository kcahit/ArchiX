using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.Library.Web.Tests.Behaviors.ValidationBehavior
{
 public sealed record VbRequest(string? Name) : ArchiX.Library.Web.Abstractions.Interfaces.IRequest<string>;
 public sealed class VbHandler : ArchiX.Library.Web.Abstractions.Interfaces.IRequestHandler<VbRequest, string>
 {
 public Task<string> HandleAsync(VbRequest request, CancellationToken cancellationToken) => Task.FromResult($"Hello, {request.Name}!");
 }
 public sealed class VbValidator : AbstractValidator<VbRequest>
 {
 public VbValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Name required");
 }
 public class ValidationBehaviorTests
 {
 [Fact]
 public async Task Valid_request_returns_handler_result()
 {
 var services = new ServiceCollection();
 services.AddSingleton<ArchiX.Library.Web.Abstractions.Interfaces.IMediator, ArchiX.Library.Web.Pipeline.Mediator>();
 services.AddSingleton(typeof(ArchiX.Library.Web.Abstractions.Interfaces.IPipelineBehavior<,>), typeof(ArchiX.Library.Web.Behaviors.ValidationBehavior<,>));
 services.AddTransient<ArchiX.Library.Web.Abstractions.Interfaces.IRequestHandler<VbRequest, string>, VbHandler>();
 services.AddTransient<IValidator<VbRequest>, VbValidator>();
 var sp = services.BuildServiceProvider();
 var mediator = sp.GetRequiredService<ArchiX.Library.Web.Abstractions.Interfaces.IMediator>();
 var result = await mediator.SendAsync(new VbRequest("Cahit"));
 Assert.Equal("Hello, Cahit!", result);
 }
 
 [Fact]
 public async Task Invalid_request_throws_ValidationException()
 {
 var services = new ServiceCollection();
 services.AddSingleton<ArchiX.Library.Web.Abstractions.Interfaces.IMediator, ArchiX.Library.Web.Pipeline.Mediator>();
 services.AddSingleton(typeof(ArchiX.Library.Web.Abstractions.Interfaces.IPipelineBehavior<,>), typeof(ArchiX.Library.Web.Behaviors.ValidationBehavior<,>));
 services.AddTransient<ArchiX.Library.Web.Abstractions.Interfaces.IRequestHandler<VbRequest, string>, VbHandler>();
 services.AddTransient<IValidator<VbRequest>, VbValidator>();
 var sp = services.BuildServiceProvider();
 var mediator = sp.GetRequiredService<ArchiX.Library.Web.Abstractions.Interfaces.IMediator>();
 await Assert.ThrowsAsync<ValidationException>(() => mediator.SendAsync(new VbRequest("")));
 }
 }
}
