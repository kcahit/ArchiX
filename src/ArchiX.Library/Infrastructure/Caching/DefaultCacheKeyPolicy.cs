// File: src/ArchiX.Library/Infrastructure/Caching/DefaultCacheKeyPolicy.cs
using System.Globalization;

namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// Varsayılan politika uygulaması. İçeride <see cref="CacheKeyBuilder"/> kullanır.
    /// C# 12 birincil kurucu (primary constructor) kullanır.
    /// </summary>
    public sealed class DefaultCacheKeyPolicy(CacheKeyPolicyOptions options) : ICacheKeyPolicy
    {
        private readonly CacheKeyPolicyOptions _opt =
            options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>Sadece parçalarla kısayol.</summary>
        public string Build(params string?[] parts)
            => Build(null, null, null, parts);

        /// <summary>
        /// Prefix / version / tenant / culture + içerik parçalarını birleştirerek anahtar üretir.
        /// </summary>
        public string Build(string? tenantId, string? culture, string? version, params string?[] parts)
        {
            // prefix
            var prefix = _opt.Prefix;

            // version
            var ver = version ?? (_opt.IncludeVersion ? _opt.DefaultVersion : null);

            // tenant
            var tenant = _opt.IncludeTenant
                ? tenantId ?? _opt.TenantAccessor?.Invoke()
                : null;

            // culture (verilmediyse CurrentUICulture.Name)
            var cultureName = _opt.IncludeCulture
                ? culture ?? _opt.CultureAccessor?.Invoke() ?? CultureInfo.CurrentUICulture.Name
                : null;

            // anahtar parçalarını topla
            var list = new List<string?>(capacity: (parts?.Length ?? 0) + 4)
            {
                prefix,
                ver is not null ? $"v:{ver}" : null,
                tenant is not null ? $"t:{tenant}" : null,
                cultureName is not null ? $"c:{cultureName}" : null
            };

            if (parts is not null && parts.Length > 0)
                list.AddRange(parts);

            // Mevcut yardımcı: CacheKeyBuilder (aynı klasörde)
            return CacheKeyBuilder.Build(200, [.. list]);
        }
    }
}
