// File: src/ArchiX.Library/Infrastructure/CacheKeyPolicyOptions.cs
namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// Cache anahtar politikası için seçenekler.
    /// Prefix, sürüm, tenant ve kültür kullanımını merkezi biçimde belirler.
    /// </summary>
    public sealed class CacheKeyPolicyOptions
    {
        /// <summary>Global ön ek. Örn: "ax", "archix". Boş veya null ise eklenmez.</summary>
        public string? Prefix { get; set; } = "ax";

        /// <summary>Sürüm bilgisini anahtara ekle.</summary>
        public bool IncludeVersion { get; set; } = false;

        /// <summary>Varsayılan sürüm değeri. IncludeVersion=true ise kullanılır.</summary>
        public string? DefaultVersion { get; set; }

        /// <summary>Tenant bilgisini anahtara ekle.</summary>
        public bool IncludeTenant { get; set; } = true;

        /// <summary>Kültür bilgisini anahtara ekle.</summary>
        public bool IncludeCulture { get; set; } = true;

        /// <summary>Tenant bilgisini sağlayan opsiyonel delege (örn. HttpContext, scoped store).</summary>
        public Func<string?>? TenantAccessor { get; set; }

        /// <summary>Kültür bilgisini sağlayan opsiyonel delege (örn. ILanguageService, CurrentUICulture).</summary>
        public Func<string?>? CultureAccessor { get; set; }
    }
}
