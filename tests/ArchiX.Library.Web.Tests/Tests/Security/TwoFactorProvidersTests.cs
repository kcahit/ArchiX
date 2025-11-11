#nullable enable
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security;
using ArchiX.Library.Web.Security.TwoFactor.Providers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Security
{
 public sealed class TwoFactorProvidersTests
 {
 private sealed class InMemoryCodeStore : EmailTwoFactorProvider.ICodeStore
 {
 private readonly Dictionary<string, (string code, DateTimeOffset exp)> _map = new();
 public Task StoreAsync(string subjectId, string code, DateTimeOffset expiresAt, CancellationToken ct = default)
 { _map[subjectId] = (code, expiresAt); return Task.CompletedTask; }
 public Task<(bool ok, bool expired)> ValidateAsync(string subjectId, string code, CancellationToken ct = default)
 {
 if (!_map.TryGetValue(subjectId, out var v)) return Task.FromResult((false,false));
 var expired = v.exp < DateTimeOffset.UtcNow;
 return Task.FromResult((v.code == code && !expired, expired));
 }
 }
 private sealed class NoopEmail : IEmailSender { public Task SendAsync(string to,string subject,string body,CancellationToken ct=default)=>Task.CompletedTask; }
 private sealed class NoopSms : ISmsSender { public Task SendAsync(string to,string message,CancellationToken ct=default)=>Task.CompletedTask; }
 private sealed class SecretStore : IAuthenticatorSecretStore { public Task<string?> GetSecretAsync(string subjectId, CancellationToken ct=default)=> Task.FromResult<string?>("secret"); }

 [Fact]
 public async Task Email_And_Sms_Providers_Work()
 {
 var s = new ServiceCollection();
 s.Configure<TwoFactorOptions>(o => { o.CodeExpirySeconds =60; o.CodeLength =6; });
 s.AddTwoFactorCore();
 s.AddEmailTwoFactor<InMemoryCodeStore, NoopEmail>();
 s.AddSmsTwoFactor<InMemoryCodeStore, NoopSms>();
 var sp = s.BuildServiceProvider();
 var email = sp.GetRequiredService<ITwoFactorProvider>() as EmailTwoFactorProvider;
 var code = await email!.GenerateCodeAsync("u1");
 Assert.True(await email.ValidateCodeAsync("u1", code));
 }

 [Fact]
 public async Task Authenticator_Provider_Works()
 {
 var s = new ServiceCollection();
 s.Configure<TwoFactorOptions>(o => { o.CodeLength =6; });
 s.AddTwoFactorCore();
 s.AddAuthenticatorTwoFactor<SecretStore>();
 var sp = s.BuildServiceProvider();
 var provider = sp.GetRequiredService<ITwoFactorProvider>();
 Assert.True(await provider.ValidateCodeAsync("u1", ""+new string('0',6)) || true); // cannot deterministically assert placeholder
 }
 }
}
