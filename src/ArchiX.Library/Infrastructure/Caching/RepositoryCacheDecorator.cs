using ArchiX.Library.Abstractions.Caching;
using ArchiX.Library.Abstractions.Persistence;
using ArchiX.Library.Infrastructure.EfCore;

namespace ArchiX.Library.Infrastructure.Caching
{
 /// <summary>
 /// Simple repository decorator that caches GetById results using ICacheService.
 /// Invalidates cache on Add/Update/Delete.
 /// </summary>
 public sealed class RepositoryCacheDecorator<T> : IRepository<T> where T : class, ArchiX.Library.Abstractions.Entities.IEntity
 {
 private readonly Repository<T> _inner; // concrete repository
 private readonly ICacheService _cache;
 private readonly TimeSpan _ttl;

 public RepositoryCacheDecorator(Repository<T> inner, ICacheService cache, TimeSpan? ttl = null)
 {
 _inner = inner ?? throw new ArgumentNullException(nameof(inner));
 _cache = cache ?? throw new ArgumentNullException(nameof(cache));
 _ttl = ttl ?? TimeSpan.FromMinutes(5);
 }

 private string KeyForId(int id) => $"repo:{typeof(T).FullName}:id:{id}";
 private string KeyForAll() => $"repo:{typeof(T).FullName}:all"; // aggregate list key

 public async Task<IEnumerable<T>> GetAllAsync()
 {
 // Cache the full list; typical read-most use cases benefit. Invalidated on mutations.
 return await _cache.GetOrSetAsync<IEnumerable<T>>(KeyForAll(), async ct => await _inner.GetAllAsync().ConfigureAwait(false), absoluteExpiration: _ttl, cacheNull: false, ct: CancellationToken.None).ConfigureAwait(false);
 }

 public Task<T?> GetByIdAsync(int id)
 {
 var key = KeyForId(id);
 return _cache.GetOrSetAsync<T?>(key, ct => _inner.GetByIdAsync(id)!, absoluteExpiration: _ttl, cacheNull: true, ct: CancellationToken.None);
 }

 public async Task AddAsync(T entity, int userId)
 {
 await _inner.AddAsync(entity, userId).ConfigureAwait(false);
 // Invalidate single + aggregate keys
 if (entity is ArchiX.Library.Entities.BaseEntity be)
 {
 var key = KeyForId(be.Id);
 await _cache.RemoveAsync(key).ConfigureAwait(false);
 }
 await _cache.RemoveAsync(KeyForAll()).ConfigureAwait(false);
 }

 public async Task UpdateAsync(T entity, int userId)
 {
 await _inner.UpdateAsync(entity, userId).ConfigureAwait(false);
 if (entity is ArchiX.Library.Entities.BaseEntity be)
 {
 var key = KeyForId(be.Id);
 await _cache.RemoveAsync(key).ConfigureAwait(false);
 }
 await _cache.RemoveAsync(KeyForAll()).ConfigureAwait(false);
 }

 public async Task DeleteAsync(int id, int userId)
 {
 await _inner.DeleteAsync(id, userId).ConfigureAwait(false);
 var key = KeyForId(id);
 await _cache.RemoveAsync(key).ConfigureAwait(false);
 await _cache.RemoveAsync(KeyForAll()).ConfigureAwait(false);
 }
 }
}
