// File: src/ArchiX.Library/Infrastructure/RedisSerializationOptions.cs
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// Redis'e yazılan/okunan JSON verisi için serileştirme seçenekleri.
    /// Options pattern ile kullanılmak üzere parametresiz kurucu içerir.
    /// </summary>
    public sealed class RedisSerializationOptions
    {
        /// <summary>
        /// System.Text.Json serileştirme seçenekleri. Varsayılanı <see cref="CreateDefault"/> değeridir.
        /// </summary>
        public JsonSerializerOptions Json { get; set; } = CreateDefault();

        /// <summary>
        /// Options pattern'in örnek oluşturabilmesi için zorunlu parametresiz kurucu.
        /// </summary>
        public RedisSerializationOptions() { }

        /// <summary>
        /// İsteğe bağlı kolay kurucu: özel <paramref name="json"/> ayarını doğrudan atar.
        /// </summary>
        public RedisSerializationOptions(JsonSerializerOptions json)
        {
            Json = json ?? throw new ArgumentNullException(nameof(json));
        }

        /// <summary>
        /// ArchiX için varsayılan JSON serileştirme seçenekleri.
        /// </summary>
        public static JsonSerializerOptions CreateDefault()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
                AllowTrailingCommas = true,
                // id/anahtar gibi değerlerde geniş karakter desteği
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }
    }
}
