namespace ArchiX.Library.Services.Parameters
{
    /// <summary>
    /// #57 Parametre servisi. DB'den parametre okur, fallback uygular ve cache'ler.
    /// </summary>
    public interface IParameterService
    {
        /// <summary>
        /// Parametreyi deserialize ederek tipli nesne olarak döndürür.
        /// ApplicationId için değer yoksa ApplicationId=1'e fallback yapar.
        /// </summary>
        Task<T?> GetParameterAsync<T>(string group, string key, int applicationId, CancellationToken ct = default)
            where T : class;

        /// <summary>
        /// Parametre değerini string olarak döndürür (raw JSON veya primitive değer).
        /// ApplicationId için değer yoksa ApplicationId=1'e fallback yapar.
        /// </summary>
        Task<string?> GetParameterValueAsync(string group, string key, int applicationId, CancellationToken ct = default);

        /// <summary>
        /// Parametre değerini günceller veya oluşturur.
        /// </summary>
        Task SetParameterAsync(string group, string key, int applicationId, string value, CancellationToken ct = default);

        /// <summary>
        /// Belirtilen parametre için cache'i temizler.
        /// </summary>
        void InvalidateCache(string group, string key);

        /// <summary>
        /// Tüm parametre cache'ini temizler.
        /// </summary>
        void InvalidateAllCache();
    }
}
