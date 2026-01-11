namespace ArchiX.Library.Abstractions.Caching;

/// <summary>
/// Cache anahtarlarý için politika sözleþmesi.
/// Prefix / tenant / culture / version parçalarýný standartlaþtýrýr.
/// </summary>
public interface ICacheKeyPolicy
{
 /// <summary>
 /// Politika kurallarýna göre cache anahtarý üretir.
 /// Parametreler null ise opsiyonel accessor'lardan veya varsayýlandan alýnýr.
 /// </summary>
 string Build(
 string? tenantId = null,
 string? culture = null,
 string? version = null,
 params string?[] parts);

 /// <summary>
 /// Sadece içerik parçalarýndan anahtar üretir; tenant/culture/version politikadan gelir.
 /// </summary>
 string Build(params string?[] parts);
}
