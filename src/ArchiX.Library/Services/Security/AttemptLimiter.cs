#nullable enable
using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.Caching.Memory;

namespace ArchiX.Library.Services.Security
{
    public sealed class AttemptLimiter : IAttemptLimiter
    {
        private readonly IMemoryCache _cache;
        private readonly AttemptLimiterOptions _opts;
        private static readonly string CacheKeyPrefix = "AttemptLimiter:";

        private sealed class Entry
        {
            public int Count;
            public DateTimeOffset FirstAt;
            public DateTimeOffset? BlockUntil;
        }

        public AttemptLimiter(IMemoryCache cache, AttemptLimiterOptions opts)
        {
            _cache = cache;
            _opts = opts;
        }

        public Task<bool> TryBeginAsync(string subjectId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
                throw new ArgumentException("subjectId is required", nameof(subjectId));

            var key = CacheKeyPrefix + subjectId;
            var absExpire = DateTimeOffset.UtcNow + Max(_opts.Window, _opts.Cooldown);

            var entry = _cache.GetOrCreate(key, e =>
            {
                e.AbsoluteExpiration = absExpire;
                return new Entry { Count = 0, FirstAt = DateTimeOffset.UtcNow, BlockUntil = null };
            })!;

            lock (entry)
            {
                var now = DateTimeOffset.UtcNow;

                // Blocked?
                if (entry.BlockUntil.HasValue && entry.BlockUntil.Value > now)
                    return Task.FromResult(false);

                // Window geçtiyse sıfırla
                if (now - entry.FirstAt > _opts.Window)
                {
                    entry.Count = 0;
                    entry.FirstAt = now;
                    entry.BlockUntil = null;
                }

                entry.Count++;

                if (entry.Count <= _opts.MaxAttempts)
                {
                    // yenileme: en uzun süre kadar cache'te tut
                    _cache.Set(key, entry, absExpire);
                    return Task.FromResult(true);
                }

                // Limit aşıldı → cooldown
                entry.BlockUntil = now + _opts.Cooldown;
                _cache.Set(key, entry, absExpire);
                return Task.FromResult(false);
            }
        }

        public Task ResetAsync(string subjectId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
                return Task.CompletedTask;

            var key = CacheKeyPrefix + subjectId;
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        private static TimeSpan Max(TimeSpan a, TimeSpan b) => a >= b ? a : b;
    }
}