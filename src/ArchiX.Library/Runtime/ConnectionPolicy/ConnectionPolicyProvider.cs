using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ArchiX.Library.Runtime.ConnectionPolicy
{
    internal sealed class ConnectionPolicyProvider : IConnectionPolicyProvider
    {
        private readonly IConfiguration _cfg;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IMemoryCache _cache;
        private readonly string _env;

        private const string CacheKey = "ConnectionPolicyOptions";
        private const string VersionKey = "ConnectionPolicyOptions:Version";
        private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

        public ConnectionPolicyProvider(IConfiguration cfg,
                                        IDbContextFactory<AppDbContext> dbFactory,
                                        IMemoryCache cache,
                                        IHostEnvironment env)
        {
            _cfg = cfg;
            _dbFactory = dbFactory;
            _cache = cache;
            _env = env.EnvironmentName ?? "Development";
        }

        public ConnectionPolicyOptions Current => _cache.GetOrCreate(CacheKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = Ttl;
            return LoadOptions();
        })!;

        public void ForceRefresh()
        {
            _cache.Remove(CacheKey);
            _cache.Remove(VersionKey);
        }

        private ConnectionPolicyOptions LoadOptions()
        {
            var section = _cfg.GetSection("ArchiX:ConnectionPolicy");

            string[] ReadArray(string key) =>
                section.GetSection(key).Get<string[]>() ?? [];

            var options = new ConnectionPolicyOptions
            {
                Mode = section.GetValue<string>("Mode") ?? DefaultModeForEnv(_env),
                AllowedHosts = ReadArray("AllowedHosts"),
                AllowedCidrs = ReadArray("AllowedCidrs"),
                RequireEncrypt = section.GetValue<bool?>("RequireEncrypt") ?? true,
                ForbidTrustServerCertificate = section.GetValue<bool?>("ForbidTrustServerCertificate") ?? DefaultForbidTrust(_env),
                AllowIntegratedSecurity = section.GetValue<bool?>("AllowIntegratedSecurity") ?? DefaultAllowIntegratedSecurity(_env)
            };

            using (var db = _dbFactory.CreateDbContext())
            {
                var settings = db.ArchiXSettings
                                 .Where(x => x.Group == "ConnectionPolicy")
                                 .AsNoTracking()
                                 .ToList();

                var latestUpdated = settings.Count == 0
                    ? DateTimeOffset.MinValue
                    : settings.Max(x => x.UpdatedAt);

                var cachedVersion = _cache.Get<DateTimeOffset?>(VersionKey);
                if (!cachedVersion.HasValue || latestUpdated >= cachedVersion.Value)
                {
                    _cache.Set(VersionKey, latestUpdated, Ttl);
                    ApplySetting(settings, "Mode", v => options = options with { Mode = v });
                    ApplySetting(settings, "RequireEncrypt", v => options = options with { RequireEncrypt = ParseBool(v, true) });
                    ApplySetting(settings, "ForbidTrustServerCertificate", v => options = options with { ForbidTrustServerCertificate = ParseBool(v, DefaultForbidTrust(_env)) });
                    ApplySetting(settings, "AllowIntegratedSecurity", v => options = options with { AllowIntegratedSecurity = ParseBool(v, DefaultAllowIntegratedSecurity(_env)) });
                    ApplyDelimited(settings, "AllowedHosts", vals => options = options with { AllowedHosts = vals });
                    ApplyDelimited(settings, "AllowedCidrs", vals => options = options with { AllowedCidrs = vals });
                }
            }

            OverrideEnv("ARCHIX__CONNECTIONPOLICY__MODE", v => options = options with { Mode = v });
            OverrideEnv("ARCHIX__CONNECTIONPOLICY__REQUIREENCRYPT", v => options = options with { RequireEncrypt = ParseBool(v, true) });
            OverrideEnv("ARCHIX__CONNECTIONPOLICY__FORBIDTRUSTSERVERCERTIFICATE", v => options = options with { ForbidTrustServerCertificate = ParseBool(v, DefaultForbidTrust(_env)) });
            OverrideEnv("ARCHIX__CONNECTIONPOLICY__ALLOWINTEGRATEDSECURITY", v => options = options with { AllowIntegratedSecurity = ParseBool(v, DefaultAllowIntegratedSecurity(_env)) });
            OverrideEnv("ARCHIX__CONNECTIONPOLICY__ALLOWEDHOSTS", v => options = options with { AllowedHosts = SplitList(v) });
            OverrideEnv("ARCHIX__CONNECTIONPOLICY__ALLOWEDCIDRS", v => options = options with { AllowedCidrs = SplitList(v) });

            return options;
        }

        private static void ApplySetting(List<ArchiXSetting> settings, string key, Action<string> apply)
        {
            var item = settings.FirstOrDefault(x => x.Key == $"ConnectionPolicy:{key}");
            if (item != null && !string.IsNullOrWhiteSpace(item.Value))
                apply(item.Value.Trim());
        }

        private static void ApplyDelimited(List<ArchiXSetting> settings, string key, Action<string[]> apply)
        {
            var item = settings.FirstOrDefault(x => x.Key == $"ConnectionPolicy:{key}");
            if (item != null && !string.IsNullOrWhiteSpace(item.Value))
                apply(SplitList(item.Value));
        }

        private static string[] SplitList(string raw) =>
            raw.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        private static void OverrideEnv(string envKey, Action<string> apply)
        {
            var v = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(v)) apply(v.Trim());
        }

        private static bool ParseBool(string raw, bool fallback) =>
            bool.TryParse(raw, out var b) ? b : fallback;

        private static string DefaultModeForEnv(string env) =>
            env.Equals("Development", StringComparison.OrdinalIgnoreCase) ? "Warn" : "Enforce";

        private static bool DefaultForbidTrust(string env) =>
            !env.Equals("Development", StringComparison.OrdinalIgnoreCase);

        private static bool DefaultAllowIntegratedSecurity(string env) =>
            env.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }
}
