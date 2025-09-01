namespace ArchiX.Library.Result;

/// <summary>
/// Hata ciddiyet seviyelerini belirtir.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Hata yok (varsayılan).
    /// </summary>
    None = 0,

    /// <summary>
    /// Bilgilendirme seviyesinde hata.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Uyarı seviyesinde hata.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Hata (işlem başarısız oldu).
    /// </summary>
    Error = 3,

    /// <summary>
    /// Kritik hata (sistemsel veya ciddi kesinti).
    /// </summary>
    Critical = 4
}
