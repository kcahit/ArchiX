using System.Text.Json;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ArchiX.Library.Runtime.Security;

internal sealed class PasswordPolicyProvider : IPasswordPolicyProvider
{
    private readonly IMemoryCache _cache;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    private static string CacheKey(int appId) => $"PasswordPolicy:{appId}";

    public PasswordPolicyProvider(IMemoryCache cache, IDbContextFactory<AppDbContext> dbFactory)
    {
        _cache = cache;
        _dbFactory = dbFactory;
    }

    public ValueTask<PasswordPolicyOptions> GetAsync(int applicationId = 1, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey(applicationId), out PasswordPolicyOptions? cached) && cached is not null)
            return ValueTask.FromResult(cached);

        return LoadAsync(applicationId, ct);
    }

    public void Invalidate(int applicationId = 1) => _cache.Remove(CacheKey(applicationId));

    private async ValueTask<PasswordPolicyOptions> LoadAsync(int appId, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var entity = await db.Parameters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApplicationId == appId && x.Group == "Security" && x.Key == "PasswordPolicy", ct)
            .ConfigureAwait(false);

        PasswordPolicyOptions opts;
        if (entity?.Value is string raw && !string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                opts = JsonSerializer.Deserialize<PasswordPolicyOptions>(raw, _jsonOpts) ?? new PasswordPolicyOptions();
            }
            catch
            {
                opts = new PasswordPolicyOptions();
            }
        }
        else
        {
            opts = new PasswordPolicyOptions();
        }

        _cache.Set(CacheKey(appId), opts);
        return opts;
    }
}
