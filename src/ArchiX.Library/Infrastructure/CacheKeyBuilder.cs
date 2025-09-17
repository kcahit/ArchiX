// File: src/ArchiX.Library/Infrastructure/CacheKeyBuilder.cs
#pragma warning disable CS1591
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ArchiX.Library.Infrastructure
{
    /// <summary>
    /// Cache anahtarları için tutarlı ve çakışmasız üretim yardımcıları.
    /// Hedefler: tutarlılık, çok kiracılı ayrıştırma, yerelleştirme ayrımı, versiyon kırma,
    /// uzun anahtarların güvenli kısaltılması.
    /// </summary>
    public static class CacheKeyBuilder
    {
        public static string Build(int maxLength = 200, params string?[] parts)
        {
            var normalized = parts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(NormalizePart)
                .Where(p => p.Length > 0)
                .ToArray();

            var key = string.Join(':', normalized);

            if (key.Length <= maxLength) return key;

            var prefixLen = Math.Max(8, maxLength - (1 + 16)); // 16 hex = 8 byte hash
            var prefix = key[..Math.Min(prefixLen, key.Length)];
            var hash = ShortHash(key);
            return $"{prefix}:{hash}";
        }

        /// <summary>
        /// Örn: tenantA:tr-tr:v2:product:42
        /// </summary>
        public static string Of(
            string? tenant = null,
            string? culture = null,
            string? version = null,
            int maxLength = 200,
            params string?[] segments)
        {
            var cultureNorm = NormalizeCulture(culture);

            var all = new string?[3 + segments.Length];
            all[0] = tenant;
            all[1] = cultureNorm;
            all[2] = version;
            if (segments.Length > 0)
                Array.Copy(segments, 0, all, 3, segments.Length);

            return Build(maxLength, all);
        }

        /// <summary>
        /// Örn: tenantA:tr-tr:v1:Product:42  (typeof(T).Name → "Product")
        /// </summary>
        public static string ForType<T>(
            string? tenant = null,
            string? culture = null,
            string? version = null,
            int maxLength = 200,
            params string?[] segments)
        {
            var list = new string?[1 + segments.Length];
            list[0] = typeof(T).Name;
            if (segments.Length > 0)
                Array.Copy(segments, 0, list, 1, segments.Length);

            return Of(tenant, culture, version, maxLength, list);
        }

        private static string NormalizePart(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var s = input.Trim().ToLowerInvariant();

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (ch is >= 'a' and <= 'z' || ch is >= '0' and <= '9' || ch is '-' or '_' or '.' or ':')
                {
                    sb.Append(ch);
                }
                else if (char.IsWhiteSpace(ch))
                {
                    sb.Append('-');
                }
            }

            var compact = CompactSeparators(sb.ToString(), ':');
            compact = CompactSeparators(compact, '-');

            return compact.Trim(':', '-');
        }

        private static string? NormalizeCulture(string? culture)
        {
            if (string.IsNullOrWhiteSpace(culture)) return null;
            try
            {
                var ci = CultureInfo.GetCultureInfo(culture);
                var parts = ci.Name.Split('-');
                if (parts.Length == 2)
                    return $"{parts[0].ToLowerInvariant()}-{parts[1].ToLowerInvariant()}";
                return ci.TwoLetterISOLanguageName.ToLowerInvariant();
            }
            catch
            {
                return NormalizePart(culture);
            }
        }

        private static string CompactSeparators(string s, char separator)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new StringBuilder(s.Length);
            var lastWasSep = false;
            foreach (var ch in s)
            {
                if (ch == separator)
                {
                    if (!lastWasSep) sb.Append(ch);
                    lastWasSep = true;
                }
                else
                {
                    sb.Append(ch);
                    lastWasSep = false;
                }
            }
            return sb.ToString();
        }

        /// <summary>8 byte SHA256 → 16 hex (lower).</summary>
        private static string ShortHash(string s)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
            var shortBytes = bytes.AsSpan(0, 8);
            return Convert.ToHexString(shortBytes).ToLowerInvariant();
        }
    }
}
#pragma warning restore CS1591
