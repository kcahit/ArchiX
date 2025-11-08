using System.Diagnostics.Metrics;

using ArchiX.Library.Abstractions.Caching;

using Microsoft.Extensions.Caching.Memory;

namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// <see cref="IMemoryCache"/> tabanlı <see cref="ICacheService"/> uygulaması.
    /// Metrikler: cache.memory.hit, cache.memory.miss, cache.memory.set.
    /// </summary>
    public sealed class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly Counter<long>? _hitCounter;
        private readonly Counter<long>? _missCounter;
        private readonly Counter<long>? _setCounter;

        // Marker to represent a cached null so we can distinguish "no entry" from "cached null".
        private sealed class NullMarker { }
        private static readonly NullMarker _nullMarker = new();

        /// <summary>
        /// Yeni bir <see cref="MemoryCacheService"/> oluşturur.
        /// </summary>
        /// <param name="cache">Altyapı bellek önbelleği.</param>
        /// <param name="meter">Opsiyonel metrik kaynağı.</param>
        public MemoryCacheService(IMemoryCache cache, Meter? meter = null)
        {
            ArgumentNullException.ThrowIfNull(cache);
            _cache = cache;

            if (meter != null)
            {
                _hitCounter = meter.CreateCounter<long>("cache.memory.hit");
                _missCounter = meter.CreateCounter<long>("cache.memory.miss");
                _setCounter = meter.CreateCounter<long>("cache.memory.set");
            }
        }

        private static MemoryCacheEntryOptions? CreateOptions(TimeSpan? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            if (!absoluteExpiration.HasValue && !slidingExpiration.HasValue) return null;
            var opt = new MemoryCacheEntryOptions();
            if (absoluteExpiration.HasValue) opt.AbsoluteExpirationRelativeToNow = absoluteExpiration;
            if (slidingExpiration.HasValue) opt.SlidingExpiration = slidingExpiration;
            return opt;
        }

        /// <summary>
        /// Anahtardan değeri okur. Yoksa <see langword="default"/> döner.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <returns>Değer ya da <see langword="default"/>.</returns>
        public T? Get<T>(object key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (_cache.TryGetValue(key, out object? raw))
            {
                if (ReferenceEquals(raw, _nullMarker))
                {
                    _hitCounter?.Add(1);
                    return default;
                }

                _hitCounter?.Add(1);
                return (T?)raw;
            }

            _missCounter?.Add(1);
            return default;
        }

        /// <summary>
        /// Anahtara değeri yazar. İsteğe bağlı mutlak yaşam süresi tanımlanabilir.
        /// </summary>
        /// <typeparam name="T">Değer türü.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="value">Yazılacak değer.</param>
        /// <param name="ttl">Mutlak yaşam süresi.</param>
        public void Set<T>(object key, T value, TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (value == null)
            {
                // store marker for explicit null
                var opts = CreateOptions(ttl, null);
                if (opts != null) _cache.Set(key, _nullMarker, opts);
                else _cache.Set(key, _nullMarker);
            }
            else
            {
                var opts = CreateOptions(ttl, null);
                if (opts != null) _cache.Set(key, value, opts);
                else _cache.Set(key, value);
            }

            _setCounter?.Add(1);
        }

        /// <summary>
        /// Değer yoksa <paramref name="factory"/> ile üretir, opsiyonel süre ile yazıp döner.
        /// Değer varsa önbellekten döner. <see langword="null"/> üretilirse yazılmaz.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="factory">Değer üretici.</param>
        /// <param name="ttl">Mutlak yaşam süresi.</param>
        /// <returns>Önbellekten ya da üreticiden dönen değer.</returns>
        public async Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory, TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(factory);

            if (_cache.TryGetValue(key, out object? raw))
            {
                if (ReferenceEquals(raw, _nullMarker))
                {
                    _hitCounter?.Add(1);
                    return default!;
                }

                _hitCounter?.Add(1);
                return (T)raw!;
            }

            _missCounter?.Add(1);
            var created = await factory().ConfigureAwait(false);
            // Only cache non-null by this API (legacy GetOrCreate semantics)
            if (created != null)
            {
                Set(key, created, ttl);
            }

            return created!;
        }

        /// <summary>
        /// Asenkron yazma. Mutlak ve/veya kayan süre tanımlanabilir.
        /// </summary>
        /// <typeparam name="T">Değer türü.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="value">Yazılacak değer.</param>
        /// <param name="absoluteExpiration">Mutlak yaşam süresi.</param>
        /// <param name="slidingExpiration">Kayan yaşam süresi.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Tamamlandığında döner.</returns>
        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            Set(key, value, absoluteExpiration);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Asenkron okuma. Yoksa <see langword="default"/> döner.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Değer ya da <see langword="default"/>.</returns>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Get<T>(key));
        }

        /// <summary>
        /// Asenkron silme.
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Tamamlandığında döner.</returns>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Anahtar mevcut mu. <see langword="null"/> değerli girişler mevcut sayılmaz.
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Varsa <see langword="true"/>, yoksa <see langword="false"/>.</returns>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out object? raw))
            {
                // treat cached explicit null as not present per contract
                return Task.FromResult(!ReferenceEquals(raw, _nullMarker));
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Yoksa üretici ile üretip yazar, varsa döner.
        /// <paramref name="cacheNull"/> <see langword="true"/> ise <see langword="null"/> değerler de cache’lenir.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="factory">Değer üretici.</param>
        /// <param name="absoluteExpiration">Mutlak yaşam süresi.</param>
        /// <param name="slidingExpiration">Kayan yaşam süresi.</param>
        /// <param name="cacheNull"><see langword="null"/> değerleri de yaz.</param>
        /// <param name="ct">İptal belirteci.</param>
        /// <returns>Önbellekten ya da üreticiden dönen değer.</returns>
        public Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, bool cacheNull = false, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(factory);

            if (_cache.TryGetValue(key, out object? raw))
            {
                if (ReferenceEquals(raw, _nullMarker))
                {
                    return Task.FromResult<T>(default!);
                }

                return Task.FromResult((T)raw!);
            }

            return GetAndMaybeCacheAsync();

            async Task<T> GetAndMaybeCacheAsync()
            {
                var created = await factory(ct).ConfigureAwait(false);
                if (created == null)
                {
                    if (cacheNull)
                    {
                        var opts = CreateOptions(absoluteExpiration, slidingExpiration);
                        if (opts != null) _cache.Set(key, _nullMarker, opts);
                        else _cache.Set(key, _nullMarker);
                        _setCounter?.Add(1);
                    }

                    return default!;
                }

                var options = CreateOptions(absoluteExpiration, slidingExpiration);
                if (options != null) _cache.Set(key, created, options);
                else _cache.Set(key, created);
                _setCounter?.Add(1);
                return created;
            }
        }

        /// <summary>
        /// Anahtarı ve ilişkili değeri siler.
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        public void Remove(object key)
        {
            ArgumentNullException.ThrowIfNull(key);
            _cache.Remove(key);
        }
    }
}
