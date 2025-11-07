using ArchiX.WebApplication.Abstractions.Authorizations;
using ArchiX.WebApplication.Abstractions.Delegates;
using ArchiX.WebApplication.Abstractions.Interfaces;
using ArchiX.WebApplication.Behaviors;
using ArchiX.WebApplication.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.Library.Web.Tests.Behaviors.AuthorizationBehavior
{
 public sealed class AuthorizationBehaviorTests
 {
 private static ServiceProvider Build(System.Action<IServiceCollection>? configure = null)
 {
 var services = new ServiceCollection();
 services.AddSingleton<IMediator, Mediator>();
 services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
 // Ensure an IAuthorizationService is always available so AuthorizationBehavior can be activated by DI
 services.AddSingleton<IAuthorizationService>(new FakeAuthorizationService { NextResult = true });
 configure?.Invoke(services);
 return services.BuildServiceProvider();
 }

 [Fact]
 public async Task No_attribute_allows_request()
 {
 var sp = Build(s =>
 {
 s.AddTransient<IRequestHandler<NoAuthReq, string>, NoAuthHandler>();
 });

 var med = sp.GetRequiredService<IMediator>();
 var res = await med.SendAsync(new NoAuthReq("OK"));
 Assert.Equal("OK", res);
 }

 [Fact]
 public async Task With_policies_and_authorized_allows_request()
 {
 var sp = Build(s =>
 {
 s.AddTransient<IRequestHandler<ReqWithPolicies, string>, ReqWithPoliciesHandler>();
 s.AddSingleton<IAuthorizationService>(new FakeAuthorizationService { NextResult = true });
 });

 var med = sp.GetRequiredService<IMediator>();
 var res = await med.SendAsync(new ReqWithPolicies("OK"));
 Assert.Equal("OK", res);
 }

 [Fact]
 public async Task With_policies_and_not_authorized_throws()
 {
 var sp = Build(s =>
 {
 s.AddTransient<IRequestHandler<ReqWithPolicies, string>, ReqWithPoliciesHandler>();
 s.AddSingleton<IAuthorizationService>(new FakeAuthorizationService { NextResult = false });
 });

 var med = sp.GetRequiredService<IMediator>();
 var ex = await Assert.ThrowsAnyAsync<Exception>(() => med.SendAsync(new ReqWithPolicies("X")));
 // Unwrap TargetInvocationException if present
 if (ex is System.Reflection.TargetInvocationException tie && tie.InnerException is not null) ex = tie.InnerException;
 Assert.IsType<UnauthorizedAccessException>(ex);
 Assert.Contains("ReqWithPolicies", ex.Message);
 Assert.Contains("P1", ex.Message);
 Assert.Contains("P2", ex.Message);
 }

 [Fact]
 public async Task Skips_When_No_Attribute()
 {
 var svc = new FakeAuthorizationService { NextResult = false };
 var behavior = new AuthorizationBehavior<NoAuthRequest, string>(svc);

 RequestHandlerDelegate<string> next = ct => Task.FromResult("ok");
 var result = await behavior.HandleAsync(new NoAuthRequest("v"), next, CancellationToken.None);

 Assert.Equal("ok", result);
 Assert.Equal(0, svc.CallCount);
 }

 [Fact]
 public async Task Calls_Service_And_Passes_When_Authorized()
 {
 var svc = new FakeAuthorizationService { NextResult = true };
 var behavior = new AuthorizationBehavior<AuthRequest, string>(svc);

 RequestHandlerDelegate<string> next = ct => Task.FromResult("ok");
 var result = await behavior.HandleAsync(new AuthRequest("v"), next, CancellationToken.None);

 Assert.Equal("ok", result);
 Assert.Equal(1, svc.CallCount);
 Assert.NotNull(svc.LastPolicies);
 Assert.True(svc.LastRequireAll);
 }

 [Fact]
 public async Task Throws_When_Not_Authorized()
 {
 var svc = new FakeAuthorizationService { NextResult = false };
 var behavior = new AuthorizationBehavior<AuthRequest, string>(svc);

 RequestHandlerDelegate<string> next = ct => Task.FromResult("ok");
 await Assert.ThrowsAsync<UnauthorizedAccessException>(() => behavior.HandleAsync(new AuthRequest("v"), next, CancellationToken.None));
 }

 [Fact]
 public async Task Honors_RequireAll_False()
 {
 var svc = new FakeAuthorizationService { NextResult = true };
 var behavior = new AuthorizationBehavior<AuthAnyRequest, string>(svc);

 RequestHandlerDelegate<string> next = ct => Task.FromResult("ok");
 var result = await behavior.HandleAsync(new AuthAnyRequest("v"), next, CancellationToken.None);

 Assert.Equal("ok", result);
 Assert.False(svc.LastRequireAll);
 Assert.Equal(1, svc.CallCount);
 }
 }

 public sealed record NoAuthRequest(string Value) : IRequest<string>;

 [Authorize("p1", "p2", RequireAll = false)]
 public sealed record AuthAnyRequest(string Value) : IRequest<string>;

 public sealed record NoAuthReq(string Msg) : IRequest<string>;
 public sealed class NoAuthHandler : IRequestHandler<NoAuthReq, string>
 {
 public Task<string> HandleAsync(NoAuthReq request, CancellationToken ct) => Task.FromResult(request.Msg);
 }

 [Authorize("P1", "P2")]
 public sealed record ReqWithPolicies(string Msg) : IRequest<string>;
 public sealed class ReqWithPoliciesHandler : IRequestHandler<ReqWithPolicies, string>
 {
 public Task<string> HandleAsync(ReqWithPolicies request, CancellationToken ct) => Task.FromResult(request.Msg);
 }
}
