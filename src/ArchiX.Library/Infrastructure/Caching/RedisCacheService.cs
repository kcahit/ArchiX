// File: src/ArchiX.Library/Infrastructure/Caching/RedisCacheService.cs
using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// <see cref="IDistributedCache"/> üzerinde JSON serileştirme ile çalışan önbellek servisi.
    /// Testlerde gerçek Redis yerine <c>AddDistributedMemoryCache</c> ile de kullanılabilir.
    /// </summary>
    public sealed class RedisCacheService(
        IDistributedCache cache,
        IOptions<RedisSerializationOptions> serializationOptions) : ICacheService
    {
        private readonly IDistributedCache _cache =
            cache ?? throw new ArgumentNullException(nameof(cache));

        private readonly JsonSerializerOptions _json =
            (serializationOptions ?? throw new ArgumentNullException(nameof(serializationOptions))).Value.Json;

        /// <summary>
        /// Verilen anahtar için değeri JSON olarak serileştirip önbelleğe yazar.
        /// </summary>
        /// <typeparam name="T">Yazılacak değer türü.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="value">Yazılacak değer.</param>
        /// <param name="absoluteExpiration">Mutlak sona erme süresi (now + x).</param>
        /// <param name="slidingExpiration">Kullanıldıkça uzayan sona erme süresi.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
            var opts = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };

            await _cache.SetAsync(key, bytes, opts, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Anahtardaki veriyi JSON’dan çözerek döner; yoksa <c>default</c>.
        /// </summary>
        /// <typeparam name="T">Okunacak değer türü.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Değer veya <c>null</c>.</returns>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, _json);
        }

        /// <summary>
        /// Anahtar varsa değeri döner; yoksa <paramref name="factory"/> ile üretir,
        /// <paramref name="cacheNull"/> ise <c>null</c> değerleri dahi cache’leyerek döner.
        /// </summary>
        /// <typeparam name="T">Değer türü (gerekirse <c>T</c>’yi nullable kullan).</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="factory">Değeri üretecek asenkron temsilci.</param>
        /// <param name="absoluteExpiration">Mutlak sona erme süresi.</param>
        /// <param name="slidingExpiration">Kayan sona erme süresi.</param>
        /// <param name="cacheNull"><c>true</c> ise <c>null</c> sonuçlar da saklanır.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Önbellekten okunan ya da üretilip yazılan değer.</returns>
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            bool cacheNull = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(factory);

            // 1) Varsa dön
            var cachedBytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (cachedBytes is not null)
            {
                var fromCache = JsonSerializer.Deserialize<T>(cachedBytes, _json);
                return fromCache!;
            }

            // 2) Yoksa üret
            var created = await factory(cancellationToken).ConfigureAwait(false);

            // 3) null ise ve cache'lenmeyecekse yazmadan dön
            if (created is null && !cacheNull)
                return created!;

            // 4) Yaz ve dön
            await SetAsync(key, created!, absoluteExpiration, slidingExpiration, cancellationToken).ConfigureAwait(false);
            return created!;
        }

        /// <summary>
        /// Anahtarın önbellekte olup olmadığını belirtir.
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Mevcutsa <c>true</c>, aksi halde <c>false</c>.</returns>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            return bytes is not null;
        }

        /// <summary>
        /// Anahtarı önbellekten siler (varsa).
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            return _cache.RemoveAsync(key, cancellationToken);
        }
    }
}
