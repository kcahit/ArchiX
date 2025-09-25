// File: src/ArchiX.Library/External/PingAdapterModelExtensions.cs
#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchiX.Library.External
{
    /// <summary>Ping yanıtını tipli modele dönüştüren yardımcılar.</summary>
    public static class PingAdapterModelExtensions
    {
        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// /status yanıtını tipli modele döndürür.
        /// JSON parse başarısız olursa metni <see cref="PingStatus.Text"/> alanına koyar.
        /// </summary>
        /// <param name="adapter">IPingAdapter bağımlılığı.</param>
        /// <param name="ct">İptal belirteci.</param>
        public static async Task<PingStatus> GetStatusAsync(this IPingAdapter adapter, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(adapter);
            var text = await adapter.GetStatusTextAsync(ct).ConfigureAwait(false);

            // JSON parse denemesi
            try
            {
                var model = JsonSerializer.Deserialize<PingStatus>(text, JsonOpts);
                if (model is not null) return model with { Text = text };
            }
            catch
            {
                // yoksay
            }

            // Düz metin
            return new PingStatus(Text: text, Service: null, Version: null, Uptime: null);
        }
    }

    /// <summary>Ping /status çıktısı için sade model.</summary>
    /// <param name="Text">Ham yanıt metni.</param>
    /// <param name="Service">Servis adı (varsa JSON’dan).</param>
    /// <param name="Version">Sürüm (varsa JSON’dan).</param>
    /// <param name="Uptime">Çalışma süresi (varsa JSON’dan).</param>
    public sealed record PingStatus(
        string Text,
        [property: JsonPropertyName("service")] string? Service,
        [property: JsonPropertyName("version")] string? Version,
        [property: JsonPropertyName("uptime")] string? Uptime
    );
}
