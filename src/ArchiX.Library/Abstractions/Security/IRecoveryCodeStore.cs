#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 public interface IRecoveryCodeStore
 {
 Task<IReadOnlyCollection<string>> GetCodesAsync(string subjectId, CancellationToken ct = default);
 Task SetCodesAsync(string subjectId, IEnumerable<string> codes, CancellationToken ct = default);
 Task<bool> ConsumeCodeAsync(string subjectId, string code, CancellationToken ct = default);
 }
}
