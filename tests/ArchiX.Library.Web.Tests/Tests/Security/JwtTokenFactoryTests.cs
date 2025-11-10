#nullable enable
using System.Security.Claims;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security.Jwt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Security
{
 public class JwtTokenFactoryTests
 {
 [Fact]
 public async Task CreateTokenPair_Returns_Access_And_Refresh()
 {
 var services = new ServiceCollection();
 services.Configure<JwtOptions>(o =>
 {
 o.Issuer = "issuer";
 o.Audience = "aud";
 o.SigningKey = new string('x',64);
 o.AccessTokenMinutes =1;
 o.RefreshTokenDays =1;
 });
 services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();
 var sp = services.BuildServiceProvider();
 var factory = sp.GetRequiredService<IJwtTokenFactory>();
 var pair = await factory.CreateTokenPairAsync(new TokenDescriptor
 {
 SubjectId = "user1",
 SubjectName = "User One",
 Claims = new() { new Claim("custom","v") },
 Scopes = new [] {"scope1"}
 });
 Assert.False(string.IsNullOrWhiteSpace(pair.AccessToken));
 Assert.False(string.IsNullOrWhiteSpace(pair.RefreshToken));
 Assert.True(pair.AccessExpiresAt > DateTimeOffset.UtcNow);
 Assert.True(pair.RefreshExpiresAt > DateTimeOffset.UtcNow);
 }
 }
}
