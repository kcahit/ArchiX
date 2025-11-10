#nullable enable

namespace ArchiX.Library.Abstractions.Security
{
 /// <summary>Refresh token saklama ve rotasyon yönetimi.</summary>
 public interface IRefreshTokenStore
 {
 Task StoreAsync(string subjectId, string refreshToken, DateTimeOffset expiresAt, CancellationToken ct = default);
 Task<bool> ValidateAsync(string subjectId, string refreshToken, CancellationToken ct = default);
 Task RevokeAsync(string subjectId, string refreshToken, CancellationToken ct = default);
 }
}
