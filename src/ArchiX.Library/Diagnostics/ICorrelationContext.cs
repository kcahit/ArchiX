namespace ArchiX.Library.Diagnostics
{
    public interface ICorrelationContext : ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext { }

    /// <summary>
    /// Basit korelasyon modeli implementasyonu.
    /// </summary>
    public sealed class CorrelationContext : ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext
    {
        /// <summary>
        /// Korelasyon kimliği.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// İzleme (Trace) kimliği.
        /// </summary>
        public string? TraceId { get; }

        /// <summary>
        /// Yeni bir <see cref="CorrelationContext"/> oluşturur.
        /// Eğer <paramref name="correlationId"/> null veya boş ise otomatik olarak yeni bir GUID atanır.
        /// </summary>
        /// <param name="correlationId">Korelasyon kimliği (opsiyonel).</param>
        /// <param name="traceId">Trace kimliği (opsiyonel).</param>
        public CorrelationContext(string? correlationId = null, string? traceId = null)
        {
            CorrelationId = string.IsNullOrWhiteSpace(correlationId)
                ? Guid.NewGuid().ToString("n")
                : correlationId!;
            TraceId = traceId;
        }
    }

    /// <summary>
    /// Ambient (AsyncLocal) tabanlı korelasyon tutucu.
    /// Korelasyon bilgisini istek zinciri boyunca taşımak için kullanılır.
    /// </summary>
    public static class Correlation
    {
        private static readonly AsyncLocal<ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext?> _ambient = new();

        /// <summary>
        /// Geçerli korelasyon bağlamı (ambient).
        /// </summary>
        public static ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext? Ambient => _ambient.Value;

        /// <summary>
        /// Yeni bir korelasyon kapsamı başlatır.
        /// </summary>
        /// <param name="correlationId">Korelasyon kimliği (opsiyonel).</param>
        /// <param name="traceId">Trace kimliği (opsiyonel).</param>
        /// <returns>IDisposable scope nesnesi.</returns>
        public static IDisposable BeginScope(string? correlationId = null, string? traceId = null)
        {
            var previous = _ambient.Value;
            _ambient.Value = new CorrelationContext(correlationId, traceId);
            return new Scope(previous);
        }

        /// <summary>
        /// Korelasyon scope’unu yöneten iç sınıf.
        /// Dispose çağrıldığında önceki bağlam geri yüklenir.
        /// </summary>
        private sealed class Scope : IDisposable
        {
            private readonly ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext? _previous;

            /// <summary>
            /// Yeni bir Scope oluşturur.
            /// </summary>
            /// <param name="previous">Önceki korelasyon bağlamı.</param>
            public Scope(ArchiX.Library.Abstractions.Diagnostics.ICorrelationContext? previous) => _previous = previous;

            /// <summary>
            /// Dispose edildiğinde önceki korelasyon bağlamını geri yükler.
            /// </summary>
            public void Dispose() => _ambient.Value = _previous;
        }
    }
}
