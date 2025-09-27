// File: src/ArchiX.Library/External/PingAdapterOptions.cs
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace ArchiX.Library.External
{
    /// <summary><see cref="IPingAdapter"/> için yapılandırma seçenekleri.</summary>
    /// <param name="BaseAddress">Dış servisin mutlak kök adresi.</param>
    /// <param name="TimeoutSeconds">İsteğe bağlı zaman aşımı (saniye, 1–300).</param>
    public sealed class PingAdapterOptions(string BaseAddress, int? TimeoutSeconds = null)
    {
        /// <summary>Dış servisin mutlak kök adresi.</summary>
        [Required, Url]
        public string BaseAddress { get; } = BaseAddress ?? throw new ArgumentNullException(nameof(BaseAddress));

        /// <summary>İsteğe bağlı zaman aşımı (saniye).</summary>
        [Range(1, 300)]
        public int? TimeoutSeconds { get; } = TimeoutSeconds;

        /// <summary><see cref="BaseAddress"/> değerinden mutlak <see cref="Uri"/> oluşturur.</summary>
        public Uri GetBaseUri() => new(BaseAddress, UriKind.Absolute);

        /// <summary><see cref="TimeoutSeconds"/> değerini <see cref="TimeSpan"/>’e dönüştürür.</summary>
        public TimeSpan? GetTimeout() => TimeoutSeconds is { } s ? TimeSpan.FromSeconds(s) : null;
    }
}
