#nullable enable
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security.TwoFactor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Security
{
 public sealed class TwoFactorCoordinatorTests
 {
 private sealed class FakeProvider : ITwoFactorProvider
 {
 public TwoFactorChannel Channel { get; init; }
 private string? _last;
 public Task<string> GenerateCodeAsync(string subjectId, CancellationToken ct = default)
 {
 _last = "123456";
 return Task.FromResult(_last);
 }
 public Task<bool> ValidateCodeAsync(string subjectId, string code, CancellationToken ct = default) => Task.FromResult(code == _last);
 }

 [Fact]
 public async Task Start_And_Verify_Flow_Succeeds()
 {
 var services = new ServiceCollection();
 services.AddLogging();
 services.AddSingleton<ITwoFactorProvider>(new FakeProvider{ Channel = TwoFactorChannel.Email});
 services.AddSingleton<ITwoFactorProvider>(new FakeProvider{ Channel = TwoFactorChannel.Sms});
 services.AddSingleton<ITwoFactorCoordinator, TwoFactorCoordinator>();
 var sp = services.BuildServiceProvider();
 var coord = sp.GetRequiredService<ITwoFactorCoordinator>();
 var start = await coord.StartAsync("user1", TwoFactorChannel.Email);
 Assert.True(start.Succeeded);
 var verify = await coord.VerifyAsync("user1", "123456");
 Assert.True(verify.Succeeded);
 }

 [Fact]
 public async Task Start_Fails_For_Unregistered_Channel()
 {
 var services = new ServiceCollection();
 services.AddLogging();
 services.AddSingleton<ITwoFactorProvider>(new FakeProvider{ Channel = TwoFactorChannel.Email});
 services.AddSingleton<ITwoFactorCoordinator, TwoFactorCoordinator>();
 var sp = services.BuildServiceProvider();
 var coord = sp.GetRequiredService<ITwoFactorCoordinator>();
 var start = await coord.StartAsync("user1", TwoFactorChannel.Sms);
 Assert.False(start.Succeeded);
 Assert.Equal("2FA_CHANNEL_NOT_REGISTERED", start.ErrorCode);
 }
 }
}
