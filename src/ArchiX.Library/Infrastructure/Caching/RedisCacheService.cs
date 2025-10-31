using System.Diagnostics.Metrics;
using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

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

        // -------- ICacheService: senkron API (object key) --------

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
            return JsonSerializer.Deserialize<T>(bytes, _json);
        }

        /// <inheritdoc />
        public void Set<T>(object key, T value, TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            var k = key.ToString();
            ArgumentException.ThrowIfNullOrEmpty(k);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
            var opts = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            _cache.Set(k, bytes, opts);
            _set?.Add(1);
        }

        /// <inheritdoc />
        public async Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory, TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(factory);

            var existing = Get<T>(key);
            if (existing is not null) return existing;

            var created = await factory().ConfigureAwait(false);
            if (created is null) return created!;

            Set(key, created, ttl);
            return created;
        }

        /// <inheritdoc />
        public void Remove(object key)
        {
            ArgumentNullException.ThrowIfNull(key);
            var k = key.ToString();
            ArgumentException.ThrowIfNullOrEmpty(k);
            _cache.Remove(k);
        }

        // -------- ICacheService: async yardımcı API (string key) --------

        /// <summary>Değeri yazar.</summary>
        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
            var opts = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };

            await _cache.SetAsync(key, bytes, opts, ct).ConfigureAwait(false);
            _set?.Add(1);
        }

        /// <summary>Değeri okur. Yoksa <see langword="default"/>.</summary>
        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);

            var bytes = await _cache.GetAsync(key, ct).ConfigureAwait(false);
            if (bytes is null)
            {
                _miss?.Add(1);
                return default;
            }

            _hit?.Add(1);
            return JsonSerializer.Deserialize<T>(bytes, _json);
        }

        /// <summary>Anahtar mevcut mu. Null içerikler mevcut sayılmaz.</summary>
        public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);

            var bytes = await _cache.GetAsync(key, ct).ConfigureAwait(false);
            if (bytes is null || bytes.Length == 0) return false;

            var token = JsonSerializer.Deserialize<object?>(bytes, _json);
            return token is not null;
        }

        /// <summary>Anahtarı siler.</summary>
        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            return _cache.RemoveAsync(key, ct);
        }

        /// <summary>Yoksa üretip yazar, varsa döner. Null sonuçlar yazılmaz.</summary>
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? absoluteTtl = null,
            TimeSpan? slidingTtl = null,
            bool cacheNull = false,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(factory);

            var existing = await GetAsync<T>(key, ct).ConfigureAwait(false);
            if (existing is not null) return existing;

            var created = await factory(ct).ConfigureAwait(false);
            if (created is null && !cacheNull) return created!;

            await SetAsync(key, created, absoluteTtl, slidingTtl, ct).ConfigureAwait(false);
            return created;
        }
    }
}
