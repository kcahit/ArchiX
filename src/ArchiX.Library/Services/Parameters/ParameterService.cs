using System.Text.Json;
using ArchiX.Library.Abstractions.Caching;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Infrastructure.Parameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Services.Parameters
{
    /// <summary>
    /// #57 Parametre servisi implementasyonu.
    /// DB'den parametre okur, fallback uygular ve cache'ler.
    /// </summary>
    public sealed class ParameterService : IParameterService
    {
        private readonly AppDbContext _db;
        private readonly ICacheService _cache;
        private readonly ILogger<ParameterService> _logger;
        private readonly ParameterRefreshOptions _refreshOptions;

        // Cache key prefix
        private const string CachePrefix = "Param";

        // Cached JsonSerializerOptions
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public ParameterService(
            AppDbContext db,
            ICacheService cache,
            ILogger<ParameterService> logger,
            ParameterRefreshOptions refreshOptions)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
            _refreshOptions = refreshOptions;
        }

        public async Task<T?> GetParameterAsync<T>(string group, string key, int applicationId, CancellationToken ct = default)
            where T : class
        {
            var cacheKey = BuildCacheKey(group, key, applicationId);

            // 1. Cache check
            var cached = _cache.Get<T>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Parameter cache hit: {CacheKey}", cacheKey);
                return cached;
            }

            // 2. DB query with fallback
            var value = await GetParameterValueWithFallbackAsync(group, key, applicationId, ct);

            if (value == null)
            {
                _logger.LogWarning("Parameter value not found: Group={Group}, Key={Key}, ApplicationId={ApplicationId}", group, key, applicationId);
                return null;
            }

            // 3. Deserialize
            T? result;
            try
            {
                result = JsonSerializer.Deserialize<T>(value, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize parameter: Group={Group}, Key={Key}, Value={Value}", group, key, value);
                throw new InvalidOperationException($"Failed to deserialize parameter '{group}/{key}' to type {typeof(T).Name}", ex);
            }

            // 4. Cache with appropriate TTL
            if (result != null)
            {
                var ttl = GetTtlForGroup(group);
                _cache.Set(cacheKey, result, ttl);
                _logger.LogDebug("Parameter cached: {CacheKey}, TTL={TTL}s", cacheKey, ttl.TotalSeconds);
            }

            return result;
        }

        public async Task<string?> GetParameterValueAsync(string group, string key, int applicationId, CancellationToken ct = default)
        {
            var cacheKey = BuildCacheKey(group, key, applicationId);

            // 1. Cache check
            var cached = _cache.Get<string>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Parameter value cache hit: {CacheKey}", cacheKey);
                return cached;
            }

            // 2. DB query with fallback
            var value = await GetParameterValueWithFallbackAsync(group, key, applicationId, ct);

            // 3. Cache
            if (value != null)
            {
                var ttl = GetTtlForGroup(group);
                _cache.Set(cacheKey, value, ttl);
                _logger.LogDebug("Parameter value cached: {CacheKey}, TTL={TTL}s", cacheKey, ttl.TotalSeconds);
            }

            return value;
        }

        public async Task SetParameterAsync(string group, string key, int applicationId, string value, CancellationToken ct = default)
        {
            // 1. Find or create parameter definition
            var param = await _db.Parameters
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Group == group && p.Key == key, ct) ?? throw new ParameterNotFoundException(group, key);

            // 2. Find or create parameter application
            var paramApp = param.Applications.FirstOrDefault(a => a.ApplicationId == applicationId);

            if (paramApp == null)
            {
                // Create new
                paramApp = new ParameterApplication
                {
                    ParameterId = param.Id,
                    ApplicationId = applicationId,
                    Value = value,
                    StatusId = BaseEntity.ApprovedStatusId,
                    CreatedBy = 0, // TODO: Get from current user
                    LastStatusBy = 0,
                    IsProtected = false,
                    RowId = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _db.ParameterApplications.Add(paramApp);
            }
            else
            {
                // Update existing
                paramApp.Value = value;
                paramApp.UpdatedAt = DateTimeOffset.UtcNow;
                paramApp.UpdatedBy = 0; // TODO: Get from current user
            }

            await _db.SaveChangesAsync(ct);

            // 3. Invalidate cache
            InvalidateCache(group, key);

            _logger.LogInformation("Parameter updated: Group={Group}, Key={Key}, ApplicationId={ApplicationId}", group, key, applicationId);
        }

        public void InvalidateCache(string group, string key)
        {
            // Invalidate all applicationId variants
            // Note: This is a simplified approach. In production, you might want to track all cached variants.
            var pattern = $"{CachePrefix}:{group}:{key}:*";
            _logger.LogDebug("Invalidating parameter cache: {Pattern}", pattern);

            // For now, we'll just log. A more sophisticated approach would require cache key tracking.
            // Alternative: use Redis key pattern matching or maintain a cache key registry.
        }

        public void InvalidateAllCache()
        {
            _logger.LogInformation("Invalidating all parameter cache (note: current implementation requires manual clear)");
            // Note: ICacheService doesn't have a clear-all method. This would need to be implemented
            // based on the underlying cache store (Memory/Redis).
        }

        #region Private Helpers

        private async Task<string?> GetParameterValueWithFallbackAsync(string group, string key, int applicationId, CancellationToken ct)
        {
            // Load parameter with applications
            var param = await _db.Parameters
                .AsNoTracking()
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Group == group && p.Key == key, ct) ?? throw new ParameterNotFoundException(group, key);

            // Try applicationId first
            var appValue = param.Applications.FirstOrDefault(a => a.ApplicationId == applicationId);

            // Fallback to ApplicationId=1 if not found
            if (appValue == null && applicationId != 1)
            {
                appValue = param.Applications.FirstOrDefault(a => a.ApplicationId == 1);
                if (appValue != null)
                {
                    _logger.LogDebug("Parameter fallback to ApplicationId=1: Group={Group}, Key={Key}, RequestedAppId={ApplicationId}",
                        group, key, applicationId);
                }
            }

            if (appValue == null)
            {
                throw new ParameterValueNotFoundException(group, key, applicationId);
            }

            return appValue.Value;
        }

        private static string BuildCacheKey(string group, string key, int applicationId)
        {
            return $"{CachePrefix}:{group}:{key}:{applicationId}";
        }

        private TimeSpan GetTtlForGroup(string group)
        {
            return group switch
            {
                "UI" => _refreshOptions.GetUiCacheTtl(),
                "HTTP" => _refreshOptions.GetHttpCacheTtl(),
                "Security" => _refreshOptions.GetSecurityCacheTtl(),
                "System" => _refreshOptions.GetSecurityCacheTtl(), // Shortest TTL for system params
                _ => TimeSpan.FromMinutes(5) // Default fallback
            };
        }

        #endregion
    }
}
