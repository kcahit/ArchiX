namespace ArchiX.Library.Web.Configuration
{
    /// <summary>
    /// #57 UI timeout parametreleri (DB-driven).
    /// Session, warning ve tab request timeout değerleri.
    /// </summary>
    public sealed class UiTimeoutOptions
    {
        /// <summary>
        /// Global session timeout süresi (saniye). Varsayılan: 645.
        /// </summary>
        public int SessionTimeoutSeconds { get; set; } = 645;

        /// <summary>
        /// Session timeout uyarısı süresi (saniye). Varsayılan: 45.
        /// </summary>
        public int SessionWarningSeconds { get; set; } = 45;

        /// <summary>
        /// Tab yükleme request timeout süresi (milisaniye). Varsayılan: 30000 (30 saniye).
        /// </summary>
        public int TabRequestTimeoutMs { get; set; } = 30000;
    }
}

