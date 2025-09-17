// File: src/ArchiX.Library/Infrastructure/RedisCacheService.cs
using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;

namespace ArchiX.Library.Infrastructure
{
    /// <summary>
    /// <see cref="IDistributedCache"/> tabanlı Redis (veya başka dağıtık cache) implementasyonu.
    /// </summary>
    public sealed class RedisCacheService(IDistributedCache cache) : IRedisCacheService
    {
        private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        /// <summary>
        /// Anahtara karşılık gelen değeri döner; yoksa <c>null</c>.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            var json = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            if (json is null) return default;
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Verilen anahtar ile değeri önbelleğe yazar.
        /// </summary>
        public Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            var options = CreateOptions(absoluteExpiration, slidingExpiration);
            var json = JsonSerializer.Serialize(value);
            return _cache.SetStringAsync(key, json, options, cancellationToken);
        }

        /// <summary>
        /// Anahtarın önbellekte mevcut olup olmadığını belirtir.
        /// </summary>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            var json = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            return json is not null;
        }

        /// <summary>
        /// Verilen anahtarı önbellekten siler (yoksa no-op).
        /// </summary>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            return _cache.RemoveAsync(key, cancellationToken);
        }

        /// <summary>
        /// Anahtar yoksa <paramref name="factory"/> ile değeri üretir, önbelleğe yazar ve döner.
        /// </summary>
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

            var json = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            if (json is not null)
                return JsonSerializer.Deserialize<T>(json)!;

            var created = await factory(cancellationToken).ConfigureAwait(false);

            if (created is null && !cacheNull)
                return created!;

            var options = CreateOptions(absoluteExpiration, slidingExpiration);
            var toCache = JsonSerializer.Serialize(created);
            await _cache.SetStringAsync(key, toCache, options, cancellationToken).ConfigureAwait(false);
            return created!;
        }

        private static DistributedCacheEntryOptions CreateOptions(TimeSpan? absolute, TimeSpan? sliding)
        {
            var opts = new DistributedCacheEntryOptions();
            if (absolute.HasValue)
                opts.AbsoluteExpirationRelativeToNow = absolute;
            if (sliding.HasValue)
                opts.SlidingExpiration = sliding;
            return opts;
        }
    }
}
