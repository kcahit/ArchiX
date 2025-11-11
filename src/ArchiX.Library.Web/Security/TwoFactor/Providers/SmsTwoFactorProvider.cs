#nullable enable
using System.Security.Cryptography;
using System.Text;
using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Web.Security.TwoFactor.Providers
{
 public interface ISmsSender { Task SendAsync(string to, string message, CancellationToken ct = default); }

 public sealed class SmsTwoFactorProvider : ITwoFactorProvider
 {
 private readonly ILogger<SmsTwoFactorProvider> _logger;
 private readonly TwoFactorOptions _opt;
 private readonly EmailTwoFactorProvider.ICodeStore _store;
 private readonly ISmsSender _smsSender;
 public TwoFactorChannel Channel => TwoFactorChannel.Sms;

 public SmsTwoFactorProvider(ILogger<SmsTwoFactorProvider> logger, IOptions<TwoFactorOptions> options, EmailTwoFactorProvider.ICodeStore store, ISmsSender smsSender)
 { _logger = logger; _opt = options.Value; _store = store; _smsSender = smsSender; }

 public async Task<string> GenerateCodeAsync(string subjectId, CancellationToken ct = default)
 {
 var code = GenerateNumericCode(_opt.CodeLength);
 var exp = DateTimeOffset.UtcNow.AddSeconds(_opt.CodeExpirySeconds);
 await _store.StoreAsync(subjectId, code, exp, ct);
 await _smsSender.SendAsync(subjectId, $"Code: {code}", ct);
 _logger.LogInformation("2FA sms code created for {Subject}", subjectId);
 return code;
 }

 public async Task<bool> ValidateCodeAsync(string subjectId, string code, CancellationToken ct = default)
 {
 var (ok, expired) = await _store.ValidateAsync(subjectId, code, ct);
 if (!ok)
 {
 _logger.LogWarning("2FA sms invalid code for {Subject}", subjectId);
 return false;
 }
 if (expired)
 {
 _logger.LogWarning("2FA sms code expired for {Subject}", subjectId);
 return false;
 }
 return true;
 }

 private static string GenerateNumericCode(int len)
 {
 Span<byte> data = stackalloc byte[len];
 RandomNumberGenerator.Fill(data);
 var sb = new StringBuilder(len);
 foreach (var b in data) sb.Append(b %10);
 return sb.ToString();
 }
 }
}
