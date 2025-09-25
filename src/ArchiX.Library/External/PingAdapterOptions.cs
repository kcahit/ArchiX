// File: src/ArchiX.Library/External/PingAdapterOptions.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.External
{
    /// <summary>IPingAdapter için yapılandırma seçenekleri.</summary>
    /// <param name="BaseAddress">Dış servisin mutlak kök adresi.</param>
    /// <param name="TimeoutSeconds">İsteğe bağlı zaman aşımı (saniye, 1–300).</param>
    public sealed class PingAdapterOptions(string BaseAddress, int? TimeoutSeconds = null)
    {
        /// <summary>Dış servisin mutlak kök adresi.</summary>
        /// <remarks>Boş olamaz ve geçerli bir URL olmalıdır.</remarks>
        [Required, Url]
        public string BaseAddress { get; } = BaseAddress ?? throw new ArgumentNullException(nameof(BaseAddress));

        /// <summary>İsteğe bağlı zaman aşımı (saniye).</summary>
        /// <remarks>1 ile 300 arasında olmalıdır; boş ise HttpClient varsayılanı kullanılır.</remarks>
        [Range(1, 300)]
        public int? TimeoutSeconds { get; } = TimeoutSeconds;

        /// <summary><see cref="BaseAddress"/> değerinden mutlak <see cref="Uri"/> oluşturur.</summary>
        public Uri GetBaseUri() => new(BaseAddress, UriKind.Absolute);

        /// <summary><see cref="TimeoutSeconds"/> değerini <see cref="TimeSpan"/>’e dönüştürür.</summary>
        public TimeSpan? GetTimeout() => TimeoutSeconds is { } s ? TimeSpan.FromSeconds(s) : null;
    }
}
