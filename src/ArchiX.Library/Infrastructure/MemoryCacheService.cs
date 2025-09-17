// File: src/ArchiX.Library/Infrastructure/MemoryCacheService.cs
using Microsoft.Extensions.Caching.Memory;

namespace ArchiX.Library.Infrastructure
{
    /// <summary>
    /// <see cref="IMemoryCache"/> tabanlı, proses-içi (in-memory) önbellek implementasyonu.
    /// </summary>
    public sealed class MemoryCacheService(IMemoryCache cache) : ICacheService
    {
        // Primary constructor parametresi alan doğrulaması ile alttaki alana atanır.
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        /// <summary>
        /// Anahtara karşılık gelen değeri döner; yoksa <c>null</c>.
        /// </summary>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            _cache.TryGetValue(key, out var value);
            return Task.FromResult((T?)value);
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
            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Anahtarın önbellekte mevcut olup olmadığını belirtir.
        /// </summary>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }

        /// <summary>
        /// Verilen anahtarı önbellekten siler (yoksa no-op).
        /// </summary>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            _cache.Remove(key);
            return Task.CompletedTask;
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

            if (_cache.TryGetValue(key, out var existing))
                return (T)existing!;

            var created = await factory(cancellationToken).ConfigureAwait(false);

            // null değerleri isteğe bağlı olarak cache'le
            if (created is null && !cacheNull)
                return created!;

            var options = CreateOptions(absoluteExpiration, slidingExpiration);
            _cache.Set(key, created, options);
            return created!;
        }

        private static MemoryCacheEntryOptions CreateOptions(TimeSpan? absolute, TimeSpan? sliding)
        {
            var opts = new MemoryCacheEntryOptions();
            if (absolute.HasValue)
                opts.SetAbsoluteExpiration(absolute.Value);
            if (sliding.HasValue)
                opts.SetSlidingExpiration(sliding.Value);
            return opts;
        }
    }
}
