using Microsoft.Extensions.Caching.Memory;
using ArchiX.Library.Abstractions.Caching;

namespace ArchiX.Library.Infrastructure.Caching
{
 /// <summary>
 /// <see cref="IMemoryCache"/> tabanlý <see cref="ICacheService"/> uygulamasý.
 /// Metrikler: cache.memory.hit, cache.memory.miss, cache.memory.set.
 /// </summary>
 public sealed class MemoryCacheService : ICacheService
 {
 private readonly IMemoryCache _cache;
 private readonly Counter<long>? _hitCounter;
 private readonly Counter<long>? _missCounter;
 private readonly Counter<long>? _setCounter;

 // Marker object to represent cached nulls so we can distinguish "no entry" from "cached null".
 private sealed class NullCacheMarker { }
 private static readonly NullCacheMarker _nullMarker = new();

 /// <summary>
 /// Yeni bir <see cref="MemoryCacheService"/> oluþturur.
 /// </summary>
 /// <param name="cache">Altyapý bellek önbelleði.</param>
 /// <param name="meter">Opsiyonel metrik kaynaðý.</param>
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

 private MemoryCacheEntryOptions? CreateOptions(TimeSpan? absoluteExpiration, TimeSpan? slidingExpiration)
 {
 if (!absoluteExpiration.HasValue && !slidingExpiration.HasValue) return null;
 var opt = new MemoryCacheEntryOptions();
 if (absoluteExpiration.HasValue) opt.AbsoluteExpirationRelativeToNow = absoluteExpiration;
 if (slidingExpiration.HasValue) opt.SlidingExpiration = slidingExpiration;
 return opt;
 }

 public T? Get<T>(object key)
 {
 ArgumentNullException.ThrowIfNull(key);

 if (_cache.TryGetValue(key, out object? raw))
 {
 if (ReferenceEquals(raw, _nullMarker))
 {
 // cached explicit null
 _hitCounter?.Add(1);
 return default;
 }

 _hitCounter?.Add(1);
 return (T?)raw;
 }

 _missCounter?.Add(1);
 return default;
 }

 public void Set<T>(object key, T value, TimeSpan? ttl = null)
 {
 ArgumentNullException.ThrowIfNull(key);

 if (value == null)
 {
 // treat as explicit cached null
 if (ttl.HasValue)
 _cache.Set(key, _nullMarker, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
 else
 _cache.Set(key, _nullMarker);
 }
 else
 {
 if (ttl.HasValue)
 _cache.Set(key, value, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
 else
 _cache.Set(key, value);
 }

 _setCounter?.Add(1);
 }

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
 if (created != null)
 {
 Set(key, created, ttl);
 }

 return created!;
 }

 public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
 {
 Set(key, value, absoluteExpiration);
 return Task.CompletedTask;
 }

 public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
 {
 return Task.FromResult(Get<T>(key));
 }

 public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
 {
 Remove(key);
 return Task.CompletedTask;
 }

 public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
 {
 if (_cache.TryGetValue(key, out object? raw))
 {
 // Treat cached explicit null as non-existent according to contract
 return Task.FromResult(!ReferenceEquals(raw, _nullMarker));
 }

 return Task.FromResult(false);
 }

 public Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, bool cacheNull = false, CancellationToken ct = default)
 {
 ArgumentNullException.ThrowIfNull(key);
 ArgumentNullException.ThrowIfNull(factory);

 if (_cache.TryGetValue(key, out object? raw))
 {
 if (ReferenceEquals(raw, _nullMarker))
 return Task.FromResult<T>(default!);

 return Task.FromResult((T)raw!);
 }

 // Not present — produce value
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
 if (options != null)
 _cache.Set(key, created, options);
 else
 _cache.Set(key, created);

 _setCounter?.Add(1);
 return created;
 }
 }

 public void Remove(object key)
 {
 ArgumentNullException.ThrowIfNull(key);
 _cache.Remove(key);
 }
 }
}
