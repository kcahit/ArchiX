#nullable enable
using System.Security.Cryptography;
using System.Text;
using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Web.Security.TwoFactor.Providers
{
 public interface IAuthenticatorSecretStore { Task<string?> GetSecretAsync(string subjectId, CancellationToken ct = default); }

 public sealed class AuthenticatorTwoFactorProvider : ITwoFactorProvider
 {
 private readonly ILogger<AuthenticatorTwoFactorProvider> _logger;
 private readonly TwoFactorOptions _opt;
 private readonly IAuthenticatorSecretStore _secretStore;
 public TwoFactorChannel Channel => TwoFactorChannel.Authenticator;

 public AuthenticatorTwoFactorProvider(ILogger<AuthenticatorTwoFactorProvider> logger, IOptions<TwoFactorOptions> options, IAuthenticatorSecretStore secretStore)
 { _logger = logger; _opt = options.Value; _secretStore = secretStore; }

 public Task<string> GenerateCodeAsync(string subjectId, CancellationToken ct = default)
 {
 // Authenticator uygulamalarý kullanýcý tarafýnda üretir; burada sadece bilgi loglanýr.
 _logger.LogInformation("Authenticator code requested for {Subject}", subjectId);
 return Task.FromResult(string.Empty);
 }

 public async Task<bool> ValidateCodeAsync(string subjectId, string code, CancellationToken ct = default)
 {
 var secret = await _secretStore.GetSecretAsync(subjectId, ct);
 if (string.IsNullOrEmpty(secret)) return false;
 // Basit TOTP doðrulama placeholder (gerçek uygulamada RFC6238 hesaplama yapýlmalý)
 var expected = SimpleTotp(secret);
 var ok = expected == code;
 if (!ok) _logger.LogWarning("Authenticator invalid code for {Subject}", subjectId);
 return ok;
 }

 private string SimpleTotp(string secret)
 {
 // NOT: Placeholder. Replace with real TOTP (HMAC-SHA1/256 time-step) implementation.
 var step = DateTimeOffset.UtcNow.ToUnixTimeSeconds() /30;
 var raw = Encoding.UTF8.GetBytes(secret + step);
 Span<byte> hash = stackalloc byte[32];
 SHA256.HashData(raw, hash);
 var sb = new StringBuilder(_opt.CodeLength);
 foreach (var b in hash[.._opt.CodeLength]) sb.Append((b %10));
 return sb.ToString();
 }
 }
}
