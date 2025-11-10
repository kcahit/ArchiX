#nullable enable
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Web.Security.Jwt
{
 public sealed class JwtTokenFactory(IOptions<JwtOptions> options) : IJwtTokenFactory
 {
 private readonly JwtOptions _opt = options.Value;
 private SymmetricSecurityKey GetKey() => new(System.Text.Encoding.UTF8.GetBytes(_opt.SigningKey));

 public async Task<TokenPair> CreateTokenPairAsync(TokenDescriptor descriptor, CancellationToken cancellationToken = default)
 {
 var (access, accessExp) = await CreateAccessTokenAsync(BuildClaims(descriptor), cancellationToken);
 var (refresh, refreshExp) = await CreateRefreshTokenAsync(descriptor.SubjectId, cancellationToken);
 return new TokenPair(access, refresh, accessExp, refreshExp);
 }

 public Task<(string token, DateTimeOffset expiresAt)> CreateAccessTokenAsync(IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
 {
 var expires = DateTimeOffset.UtcNow.AddMinutes(_opt.AccessTokenMinutes);
 var token = new JwtSecurityToken(
 issuer: _opt.Issuer,
 audience: _opt.Audience,
 claims: claims,
 notBefore: DateTime.UtcNow,
 expires: expires.UtcDateTime,
 signingCredentials: new SigningCredentials(GetKey(), _opt.Algorithm));
 var jwt = new JwtSecurityTokenHandler().WriteToken(token);
 return Task.FromResult<(string, DateTimeOffset)>((jwt, expires));
 }

 public Task<(string token, DateTimeOffset expiresAt)> CreateRefreshTokenAsync(string subjectId, CancellationToken cancellationToken = default)
 {
 var bytes = RandomNumberGenerator.GetBytes(64);
 var token = Convert.ToBase64String(bytes);
 var expires = DateTimeOffset.UtcNow.AddDays(_opt.RefreshTokenDays);
 return Task.FromResult<(string, DateTimeOffset)>((token, expires));
 }

 private static IEnumerable<Claim> BuildClaims(TokenDescriptor d)
 {
 var claims = new List<Claim>
 {
 new(JwtRegisteredClaimNames.Sub, d.SubjectId),
 new(JwtRegisteredClaimNames.UniqueName, d.SubjectName ?? d.SubjectId),
 new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
 };
 if (d.Scopes is { Length: >0 })
 {
 claims.AddRange(d.Scopes.Select(s => new Claim("scope", s)));
 }
 if (d.Claims is { Count: >0 }) claims.AddRange(d.Claims);
 return claims;
 }
 }
}
