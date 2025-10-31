using ArchiX.WebApplication.Abstractions.Authorizations;
using ArchiX.WebApplication.Abstractions.Delegates;
using ArchiX.WebApplication.Abstractions.Interfaces;
using ArchiX.WebApplication.Behaviors;

using Xunit;

namespace ArchiX.WebApplication.Tests.Behaviors.AuthorizationBehavior
{
    /// <summary>
    /// 7,0500 — AuthorizationBehavior birim testleri.
    /// </summary>
    public sealed class AuthorizationBehaviorTests
    {
        [Fact]
        public async Task Skips_When_No_Attribute()
        {
            var svc = new FakeAuthorizationService { NextResult = false }; // çağrılmamalı
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
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                behavior.HandleAsync(new AuthRequest("v"), next, CancellationToken.None));
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

    /// <summary>Attribute olmayan örnek istek.</summary>
    public sealed record NoAuthRequest(string Value) : IRequest<string>;

    /// <summary>RequireAll=false için ek örnek istek.</summary>
    [Authorize("p1", "p2", RequireAll = false)]
    public sealed record AuthAnyRequest(string Value) : IRequest<string>;
}
