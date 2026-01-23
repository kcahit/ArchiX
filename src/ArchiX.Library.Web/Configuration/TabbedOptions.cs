namespace ArchiX.Library.Web.Configuration
{
    /// <summary>
    /// #42 Tabbed navigation parametreleri (DB-driven).
    /// DB'deki UI/TabbedOptions parametresinden okunur.
    /// </summary>
    public sealed class TabbedOptions
    {
        /// <summary>
        /// Navigasyon modu (Tabbed veya FullPage). Varsayılan: Tabbed.
        /// </summary>
        public string NavigationMode { get; set; } = "Tabbed";

        /// <summary>
        /// Tabbed mode özgü ayarlar.
        /// </summary>
        public TabbedModeOptions? Tabbed { get; set; }

        /// <summary>
        /// FullPage mode özgü ayarlar.
        /// </summary>
        public FullPageModeOptions? FullPage { get; set; }
    }

    /// <summary>
    /// Tabbed mode özgü ayarlar.
    /// </summary>
    public sealed class TabbedModeOptions
    {
        /// <summary>
        /// Maksimum açık tab sayısı. Varsayılan: 15.
        /// </summary>
        public int MaxOpenTabs { get; set; } = 15;

        /// <summary>
        /// Tab otomatik kapanma süresi (dakika). Varsayılan: 10.
        /// </summary>
        public int TabAutoCloseMinutes { get; set; } = 10;

        /// <summary>
        /// Tab otomatik kapanma uyarısı süresi (saniye). Varsayılan: 30.
        /// </summary>
        public int AutoCloseWarningSeconds { get; set; } = 30;

        /// <summary>
        /// Nested tab desteği. Varsayılan: false.
        /// </summary>
        public bool EnableNestedTabs { get; set; } = false;

        /// <summary>
        /// Direct link engeli (tab context zorunluluğu). Varsayılan: true.
        /// </summary>
        public bool RequireTabContext { get; set; } = true;

        /// <summary>
        /// Max tab limitine ulaşıldığında davranış.
        /// </summary>
        public MaxTabReachedOptions? OnMaxTabReached { get; set; }

        /// <summary>
        /// Tab başlığı unique suffix ayarları.
        /// </summary>
        public TabTitleSuffixOptions? TabTitleUniqueSuffix { get; set; }
    }

    /// <summary>
    /// Max tab limitine ulaşıldığında davranış.
    /// </summary>
    public sealed class MaxTabReachedOptions
    {
        /// <summary>
        /// Davranış (Block, CloseOldest, vb.). Varsayılan: Block.
        /// </summary>
        public string Behavior { get; set; } = "Block";

        /// <summary>
        /// Kullanıcıya gösterilecek mesaj.
        /// </summary>
        public string Message { get; set; } = "Maksimum tab limiti doldu.";
    }

    /// <summary>
    /// Tab başlığı unique suffix ayarları.
    /// </summary>
    public sealed class TabTitleSuffixOptions
    {
        /// <summary>
        /// Suffix formatı (örn: "_{000}"). Varsayılan: "_{000}".
        /// </summary>
        public string Format { get; set; } = "_{000}";

        /// <summary>
        /// Başlangıç sayısı. Varsayılan: 1.
        /// </summary>
        public int Start { get; set; } = 1;
    }

    /// <summary>
    /// FullPage mode özgü ayarlar.
    /// </summary>
    public sealed class FullPageModeOptions
    {
        /// <summary>
        /// Varsayılan landing route. Varsayılan: /Dashboard.
        /// </summary>
        public string DefaultLandingRoute { get; set; } = "/Dashboard";

        /// <summary>
        /// Raporları yeni pencerede aç. Varsayılan: false.
        /// </summary>
        public bool OpenReportsInNewWindow { get; set; } = false;

        /// <summary>
        /// Kaydedilmemiş değişikliklerde onay iste. Varsayılan: true.
        /// </summary>
        public bool ConfirmOnUnsavedChanges { get; set; } = true;

        /// <summary>
        /// Deep link desteği. Varsayılan: true.
        /// </summary>
        public bool DeepLinkEnabled { get; set; } = true;

        /// <summary>
        /// Hata modu. Varsayılan: DefaultErrorPage.
        /// </summary>
        public string ErrorMode { get; set; } = "DefaultErrorPage";

        /// <summary>
        /// Keep-alive desteği. Varsayılan: true.
        /// </summary>
        public bool EnableKeepAlive { get; set; } = true;

        /// <summary>
        /// Session timeout uyarısı (saniye). Varsayılan: 60.
        /// </summary>
        public int SessionTimeoutWarningSeconds { get; set; } = 60;
    }
}
