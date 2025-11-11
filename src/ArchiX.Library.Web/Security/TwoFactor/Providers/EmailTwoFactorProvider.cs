#nullable enable
using System.Security.Cryptography;
using System.Text;
using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Web.Security.TwoFactor.Providers
{
 public interface IEmailSender { Task SendAsync(string to, string subject, string body, CancellationToken ct = default); }

 public sealed class EmailTwoFactorProvider : ITwoFactorProvider
 {
 private readonly ILogger<EmailTwoFactorProvider> _logger;
 private readonly TwoFactorOptions _opt;
 private readonly ICodeStore _store;
 private readonly IEmailSender _emailSender;
 public TwoFactorChannel Channel => TwoFactorChannel.Email;

 public interface ICodeStore
 {
 Task StoreAsync(string subjectId, string code, DateTimeOffset expiresAt, CancellationToken ct = default);
 Task<(bool ok, bool expired)> ValidateAsync(string subjectId, string code, CancellationToken ct = default);
 }

 public EmailTwoFactorProvider(ILogger<EmailTwoFactorProvider> logger, IOptions<TwoFactorOptions> options, ICodeStore store, IEmailSender emailSender)
 {
 _logger = logger; _opt = options.Value; _store = store; _emailSender = emailSender;
 }

 public async Task<string> GenerateCodeAsync(string subjectId, CancellationToken ct = default)
 {
 var code = GenerateNumericCode(_opt.CodeLength);
 var exp = DateTimeOffset.UtcNow.AddSeconds(_opt.CodeExpirySeconds);
 await _store.StoreAsync(subjectId, code, exp, ct);
 await _emailSender.SendAsync(subjectId, "Your verification code", $"Code: {code}", ct);
 _logger.LogInformation("2FA email code created for {Subject}", subjectId);
 return code;
 }

 public async Task<bool> ValidateCodeAsync(string subjectId, string code, CancellationToken ct = default)
 {
 var (ok, expired) = await _store.ValidateAsync(subjectId, code, ct);
 if (!ok)
 {
 _logger.LogWarning("2FA email invalid code for {Subject}", subjectId);
 return false;
 }
 if (expired)
 {
 _logger.LogWarning("2FA email code expired for {Subject}", subjectId);
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
