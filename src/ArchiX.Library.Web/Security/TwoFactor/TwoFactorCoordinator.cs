#nullable enable
using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Web.Security.TwoFactor
{
 public sealed class TwoFactorCoordinator(ILogger<TwoFactorCoordinator> logger, IEnumerable<ITwoFactorProvider> providers)
 : ITwoFactorCoordinator
 {
 private readonly ILogger<TwoFactorCoordinator> _logger = logger;
 private readonly IReadOnlyDictionary<TwoFactorChannel, ITwoFactorProvider> _providers = providers.ToDictionary(p => p.Channel);

 public async Task<AuthResult> StartAsync(string subjectId, TwoFactorChannel preferred, CancellationToken ct = default)
 {
 if (!_providers.TryGetValue(preferred, out var provider))
 return AuthResult.Failure("2FA_CHANNEL_NOT_REGISTERED", $"Channel {preferred} not registered");
 _ = await provider.GenerateCodeAsync(subjectId, ct).ConfigureAwait(false);
 _logger.LogInformation("2FA code generated for {Subject} via {Channel}", subjectId, preferred);
 return AuthResult.Success();
 }

 public async Task<AuthResult> VerifyAsync(string subjectId, string code, CancellationToken ct = default)
 {
 foreach (var p in _providers.Values)
 {
 if (await p.ValidateCodeAsync(subjectId, code, ct).ConfigureAwait(false))
 return AuthResult.Success();
 }
 return AuthResult.Failure("2FA_INVALID_CODE", "Invalid code");
 }
 }
}
