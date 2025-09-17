// File: src/ArchiX.Library/Infrastructure/RedisSerializationOptions.cs
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchiX.Library.Infrastructure
{
    /// <summary>
    /// Redis'e yazılan/Redis'ten okunan JSON verisinin serileştirme seçenekleri.
    /// Varsayılanlar: camelCase, null alanları yazma, kompakt çıktı.
    /// </summary>
    public sealed class RedisSerializationOptions
    {
        /// <summary>
        /// System.Text.Json serileştirme seçenekleri.
        /// </summary>
        public JsonSerializerOptions Json { get; }

        /// <summary>
        /// Özelleştirilmiş <see cref="JsonSerializerOptions"/> vermezsen,
        /// proje için uygun varsayılanlarla oluşturur.
        /// </summary>
        public RedisSerializationOptions(JsonSerializerOptions? json = null)
        {
            Json = json ?? CreateDefault();
        }

        /// <summary>
        /// ArchiX için varsayılan JSON serileştirme seçenekleri.
        /// - camelCase alan adları
        /// - null alanları yazma (WhenWritingNull)
        /// - kompakt çıktı (WriteIndented=false)
        /// - toleranslı okuma için AllowTrailingCommas=true
        /// </summary>
        public static JsonSerializerOptions CreateDefault()
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
                AllowTrailingCommas = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // id/anahtar vb. için geniş karakter desteği
            };

            // Gelecekte gerekirse açarız:
            // opts.NumberHandling = JsonNumberHandling.AllowReadingFromString;

            return opts;
        }
    }
}
