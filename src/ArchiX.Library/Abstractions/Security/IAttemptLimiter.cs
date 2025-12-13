#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 public interface IAttemptLimiter
 {
 Task<bool> TryBeginAsync(string subjectId, CancellationToken ct = default);
 Task ResetAsync(string subjectId, CancellationToken ct = default);
 }
}
