namespace ArchiX.Library.Logging
{
    /// <summary>
    /// Uygulama ve çalışma ortamı hakkında log bilgilerini tutar.
    /// </summary>
    public sealed class LogApp
    {
        /// <summary>
        /// Uygulama adı (örn: ArchiXTests.Api).
        /// </summary>
        public string? App { get; set; }

        /// <summary>
        /// Uygulama versiyonu (örn: 1.0.0+build).
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Çalışma ortamı (Development / Staging / Production).
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Çalıştığı makine adı (hostname).
        /// </summary>
        public string? Machine { get; set; }

        /// <summary>
        /// İşlem (process) kimliği.
        /// </summary>
        public int? ProcessId { get; set; }

        /// <summary>
        /// Thread kimliği.
        /// </summary>
        public int? ThreadId { get; set; }
    }
}
