// File: src/ArchiX.Library/Infrastructure/ICacheKeyPolicy.cs
namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// Cache anahtarları için politika sözleşmesi.
    /// Prefix/tenant/culture/version parçalarını standartlaştırır.
    /// </summary>
    public interface ICacheKeyPolicy
    {
        /// <summary>
        /// Politika kurallarına göre cache anahtarı üretir.
        /// Parametreler null ise opsiyonel accessor'lardan veya varsayılandan alınır.
        /// </summary>
        string Build(
            string? tenantId = null,
            string? culture = null,
            string? version = null,
            params string?[] parts);

        /// <summary>
        /// Sadece içerik parçalarından anahtar üretir; tenant/culture/version politikadan gelir.
        /// </summary>
        string Build(params string?[] parts);
    }
}
