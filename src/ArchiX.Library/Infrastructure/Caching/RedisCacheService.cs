using System.Diagnostics.Metrics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using ArchiX.Library.Abstractions.Caching;

namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// <see cref="IDistributedCache"/> üzerinde JSON serileştirme ile çalışan, metrik yayımlayan önbellek servisi.
    /// Metrikler: <c>cache.redis.hit</c>, <c>cache.redis.miss</c>, <c>cache.redis.set</c>.
    /// </summary>
    public sealed class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _json;
        private readonly Counter<long>? _hit;
        private readonly Counter<long>? _miss;
        private readonly Counter<long>? _set;

        /// <summary>
        /// Yeni bir <see cref="RedisCacheService"/> oluşturur.
        /// </summary>
        /// <param name="cache">Altyapı önbelleği.</param>
        /// <param name="serialization">Serileştirme seçenekleri.</param>
        /// <param name="meter">Opsiyonel <see cref="Meter"/>; verildiğinde metrikler yayımlanır.</param>
        public RedisCacheService(
            IDistributedCache cache,
            IOptions<RedisSerializationOptions>? serialization = null,
            Meter? meter = null)
        {
            ArgumentNullException.ThrowIfNull(cache);
            _cache = cache;

            _json = serialization?.Value?.Json ?? new JsonSerializerOptions();

            if (meter is not null)
            {
                _hit = meter.CreateCounter<long>("cache.redis.hit");
                _miss = meter.CreateCounter<long>("cache.redis.miss");
                _set = meter.CreateCounter<long>("cache.redis.set");
            }
        }

        /// <inheritdoc />
        public T? Get<T>(object key)
        {
            ArgumentNullException.ThrowIfNull(key);
            var k = key.ToString();
            ArgumentException.ThrowIfNullOrEmpty(k);

            var bytes = _cache.Get(k);
            if (bytes is null)
            {
                _miss?.Add(1);
                return default;
            }

            _hit?.Add(1);
            return Deserialize<T>(bytes);
        }

        /// <inheritdoc />
        public void Set<T>(object key, T value, TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            var k = key.ToString();
            ArgumentException.ThrowIfNullOrEmpty(k);

            var bytes = Serialize(value);
            var opts = new DistributedCacheEntryOptions();
            if (ttl.HasValue) opts.AbsoluteExpirationRelativeToNow = ttl;

            _cache.Set(k, bytes, opts);
            _set?.Add(1);
        }

        /// <summary>
        /// Anahtar ve değeri ile birlikte bir fabrika işlevi sağlayarak, önbellekten veriyi alma veya oluşturup önbelleğe yazma işlemini gerçekleştirir.
        /// </summary>
        /// <typeparam name="T">Veri tipi.</typeparam>
        /// <param name="key">Anahtar.</param>
        /// <param name="factory">Veri oluşturma işlevi.</param>
        /// <param name="ttl">Varsayılan süre bitimi.</param>
        /// <returns>Önbellekten alınan veya önbelleğe yazılan veri.</returns>
        public async Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory, TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(factory);

            var k = key.ToString();
            ArgumentException.ThrowIfNullOrEmpty(k);

            var bytes = await _cache.GetAsync(k).ConfigureAwait(false);
            if (bytes is not null)
            {
                _hit?.Add(1);
                return Deserialize<T>(bytes)!;
            }

            _miss?.Add(1);

            var created = await factory().ConfigureAwait(false);
            Set(key, created, ttl);
            return created;
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);

            var bytes = Serialize(value);
            var opts = new DistributedCacheEntryOptions();
            if (absoluteExpiration.HasValue) opts.AbsoluteExpirationRelativeToNow = absoluteExpiration;
            if (slidingExpiration.HasValue) opts.SlidingExpiration = slidingExpiration;

            await _cache.SetAsync(key, bytes, opts, cancellationToken).ConfigureAwait(false);
            _set?.Add(1);
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);

            var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (bytes is null)
            {
                _miss?.Add(1);
                return default;
            }

            _hit?.Add(1);
            return Deserialize<T>(bytes);
        }

        /// <inheritdoc />
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            return _cache.RemoveAsync(key, cancellationToken);
        }

        /// <summary>
        /// Anahtarın önbellekte var olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="key">Anahtar.</param>
        /// <param name="cancellationToken">İptal token'ı.</param>
        /// <returns>True: Anahtar mevcut, False: Anahtar mevcut değil.</returns>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            return Task.FromResult(_cache.Get(key) is not null);
        }

        /// <summary>Anahtarı siler.</summary>
        public void Remove(object key)
        {
            ArgumentNullException.ThrowIfNull(key);
            var k = key.ToString();
            if (!string.IsNullOrEmpty(k)) _cache.Remove(k);
        }

        /// <summary>Yoksa üretip yazar, varsa döner. Null sonuçlar yazılmaz.</summary>
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            bool cacheNull = false,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(factory);

            var existing = await GetAsync<T>(key, ct).ConfigureAwait(false);
            if (existing is not null) return existing;

            var created = await factory(ct).ConfigureAwait(false);
            if (created is null && !cacheNull) return created!;

            await SetAsync(key, created, absoluteExpiration, slidingExpiration, ct).ConfigureAwait(false);
            return created;
        }

        private byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, _json);
        private T? Deserialize<T>(byte[] bytes) => bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, _json);
    }
}
