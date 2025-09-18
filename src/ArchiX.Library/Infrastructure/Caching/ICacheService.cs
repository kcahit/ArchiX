// File: src/ArchiX.Library/Infrastructure/ICacheService.cs
namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// Sağlayıcı bağımsız önbellek sözleşmesi.
    /// Memory (in-proc) ve Redis (distributed) implementasyonları bu arabirimi uygular.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>Key üzerinden değeri getirir; yoksa null döner.</summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Key ile değeri yazar.
        /// Opsiyonel absolute/sliding expiration destekler.
        /// </summary>
        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default);

        /// <summary>Key mevcut mu?</summary>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>Key’i siler (yoksa no-op).</summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Değer yoksa <paramref name="factory"/> ile üretip cache’e koyar ve döner.
        /// </summary>
        Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            bool cacheNull = false,
            CancellationToken cancellationToken = default);
    }


}
