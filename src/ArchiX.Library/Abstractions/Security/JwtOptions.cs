#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 /// <summary>JWT üretimi için seçenekler.</summary>
 public sealed class JwtOptions
 {
 /// <summary>Token issuer.</summary>
 public string Issuer { get; set; } = string.Empty;
 /// <summary>Token audience.</summary>
 public string Audience { get; set; } = string.Empty;
 /// <summary>Symmetric signing key (HMAC). Production: store securely.</summary>
 public string SigningKey { get; set; } = string.Empty;
 /// <summary>Eriþim (access) token geçerlilik süresi (dakika).</summary>
 public int AccessTokenMinutes { get; set; } =60;
 /// <summary>Yenileme (refresh) token geçerlilik süresi (gün).</summary>
 public int RefreshTokenDays { get; set; } =7;
 /// <summary>Ýmza algoritmasý. Varsayýlan HS256.</summary>
 public string Algorithm { get; set; } = "HS256";
 }
}
