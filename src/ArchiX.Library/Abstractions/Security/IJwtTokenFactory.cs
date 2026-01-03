#nullable enable
using System.Security.Claims;

namespace ArchiX.Library.Abstractions.Security
{
 public interface IJwtTokenFactory
 {
 /// <summary>Access ve refresh token üretir.</summary>
 Task<TokenPair> CreateTokenPairAsync(TokenDescriptor descriptor, CancellationToken cancellationToken = default);
 /// <summary>Access token (JWT) üretir.</summary>
 Task<(string token, DateTimeOffset expiresAt)> CreateAccessTokenAsync(IEnumerable<Claim> claims, CancellationToken cancellationToken = default);
 /// <summary>Refresh token üretir (opaque, rotation için yeterli entropiyle).</summary>
 Task<(string token, DateTimeOffset expiresAt)> CreateRefreshTokenAsync(string subjectId, CancellationToken cancellationToken = default);
 }
}
