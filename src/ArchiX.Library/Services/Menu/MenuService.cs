using ArchiX.Library.Abstractions.Caching;
using ArchiX.Library.Configuration;
using MenuEntity = ArchiX.Library.Entities.Menu;
using ArchiX.Library.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Services.Menu
{
    /// <summary>
    /// Menü servisinin EF Core implementasyonu. Menüler müşteri DB'sindeki Menu tablosundan okunur.
    /// </summary>
    public sealed class MenuService<TContext> : IMenuService where TContext : DbContext
    {
        private readonly TContext _db;
        private readonly ICacheService _cache;
        private readonly ArchiXOptions _options;
        private readonly ILogger<MenuService<TContext>> _logger;

        public MenuService(TContext db, ICacheService cache, IOptions<ArchiXOptions> options, ILogger<MenuService<TContext>> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<MenuItem>> GetMenuForApplicationAsync(int applicationId, CancellationToken ct = default)
        {
            var cacheKey = $"menu_{applicationId}";
            var cached = _cache.Get<IReadOnlyList<MenuItem>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Menu cache hit for ApplicationId={ApplicationId}", applicationId);
                return cached;
            }

            var items = await _db.Set<MenuEntity>()
                .AsNoTracking()
                .Where(m => m.ApplicationId == applicationId)
                .OrderBy(m => m.SortOrder)
                .Select(m => new MenuItem
                {
                    Id = m.Id,
                    Title = m.Title,
                    Url = m.Url,
                    SortOrder = m.SortOrder,
                    ParentId = m.ParentId,
                    Icon = m.Icon
                })
                .ToListAsync(ct);

            _cache.Set(cacheKey, (IReadOnlyList<MenuItem>)items, _options.MenuCacheDuration);
            return items;
        }

        public Task InvalidateMenuCacheAsync(int applicationId, CancellationToken ct = default)
        {
            var cacheKey = $"menu_{applicationId}";
            _logger.LogDebug("Invalidating menu cache: {CacheKey}", cacheKey);
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }
    }
}
