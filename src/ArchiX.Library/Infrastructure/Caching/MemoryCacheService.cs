using System.Diagnostics.Metrics;

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

        /// <summary>
        /// Anahtardan değeri okur. Yoksa <see langword="default"/> döner.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <returns>Değer ya da <see langword="default"/>.</returns>
        public T? Get<T>(object key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (_cache.TryGetValue(key, out T? value))
            {
                _hitCounter?.Add(1);
                return value;
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

            if (ttl.HasValue)
            {
                _cache.Set(key, value, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });
            }
            else
            {
                _cache.Set(key, value);
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

            if (_cache.TryGetValue(key, out T? cached))
            {
                _hitCounter?.Add(1);
                return cached!;
            }

            _missCounter?.Add(1);
            var created = await factory().ConfigureAwait(false);

            if (created is null) return created!;
            Set(key, created, ttl);
            return created;
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
            ArgumentException.ThrowIfNullOrEmpty(key);

            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            });

            _setCounter?.Add(1);
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
            ArgumentException.ThrowIfNullOrEmpty(key);

            if (_cache.TryGetValue(key, out T? value))
            {
                _hitCounter?.Add(1);
                return Task.FromResult(value);
            }

            _missCounter?.Add(1);
            return Task.FromResult(default(T));
        }

        /// <summary>
        /// Asenkron silme.
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Tamamlandığında döner.</returns>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            _cache.Remove(key);
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
            ArgumentException.ThrowIfNullOrEmpty(key);
            var exists = _cache.TryGetValue(key, out var value) && value is not null;
            return Task.FromResult(exists);
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

            if (_cache.TryGetValue(key, out object? boxed))
            {
                _hitCounter?.Add(1);
                return (T?)boxed!;
            }

            _missCounter?.Add(1);
            var created = await factory(ct).ConfigureAwait(false);

            if (created is null && !cacheNull) return created!;

            _cache.Set(key, created, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            });
            _setCounter?.Add(1);
            return created!;
        }
    }
}
