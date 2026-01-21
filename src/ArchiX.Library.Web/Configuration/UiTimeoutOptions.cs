namespace ArchiX.Library.Web.Configuration
{
    /// <summary>
    /// UI timeout ayarları. Normalde DB'deki parametre tablosundan gelir.
    /// Şu an hard-coded, DB bağlandığında dinamik olacak.
    /// </summary>
    public sealed class UiTimeoutOptions
    {
        /// <summary>
        /// Global session timeout süresi (saniye). Varsayılan: 150 (2.5 dakika).
        /// </summary>
        public int SessionTimeoutSeconds { get; set; } = 150;

        /// <summary>
        /// Session timeout uyarısı süresi (saniye). Varsayılan: 30.
        /// Uyarı 120 saniye sonra gösterilecek (150 - 30 = 120).
        /// </summary>
        public int SessionWarningSeconds { get; set; } = 30;

        /// <summary>
        /// Tab yükleme request timeout süresi (milisaniye). Varsayılan: 30000 (30 saniye).
        /// </summary>
        public int TabRequestTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Maksimum açık tab sayısı. Varsayılan: 15.
        /// </summary>
        public int MaxOpenTabs { get; set; } = 15;
    }
}
