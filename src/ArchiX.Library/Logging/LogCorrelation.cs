namespace ArchiX.Library.Logging
{
    /// <summary>
    /// Log kaydı için korelasyon (CorrelationId, TraceId) bilgilerini tutar.
    /// </summary>
    public sealed class LogCorrelation
    {
        /// <summary>
        /// İstek ile ilişkilendirilmiş korelasyon kimliği.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// İzleme (Trace) kimliği.
        /// </summary>
        public string? TraceId { get; set; }
    }
}
