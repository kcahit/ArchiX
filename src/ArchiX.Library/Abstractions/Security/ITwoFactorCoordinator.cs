#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 /// <summary>2FA sürecini koordine eder (kod üret, gönder, doðrula).</summary>
 public interface ITwoFactorCoordinator
 {
 Task<AuthResult> StartAsync(string subjectId, TwoFactorChannel preferred, CancellationToken ct = default);
 Task<AuthResult> VerifyAsync(string subjectId, string code, CancellationToken ct = default);
 }
}
