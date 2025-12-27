#nullable enable
using System.Security.Claims;

namespace ArchiX.Library.Abstractions.Security
{
 public sealed record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessExpiresAt, DateTimeOffset RefreshExpiresAt);

 public sealed record AuthResult(bool Succeeded, string? ErrorCode = null, string? ErrorDescription = null)
 {
 public static AuthResult Success() => new(true);
 public static AuthResult Failure(string code, string? description = null) => new(false, code, description);
 }

 /// <summary>Token üretimi için parametreler.</summary>
 public sealed class TokenDescriptor
 {
 public required string SubjectId { get; init; }
 public string? SubjectName { get; init; }
 public List<Claim> Claims { get; init; } = new();
 public DateTimeOffset? AccessExpiresAt { get; init; }
 public string[]? Scopes { get; init; }
 }
}
