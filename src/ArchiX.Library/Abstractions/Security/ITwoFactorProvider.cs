#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 /// <summary>2FA kod üretim & doðrulama saðlayýcýsý.</summary>
 public interface ITwoFactorProvider
 {
 TwoFactorChannel Channel { get; }
 Task<string> GenerateCodeAsync(string subjectId, CancellationToken ct = default);
 Task<bool> ValidateCodeAsync(string subjectId, string code, CancellationToken ct = default);
 }
}
