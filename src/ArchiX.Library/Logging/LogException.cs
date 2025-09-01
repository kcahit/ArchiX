namespace ArchiX.Library.Logging;

/// <summary>
/// Uygulamada oluşan exception bilgilerini tutar.
/// </summary>
public sealed class LogException
{
    /// <summary>
    /// Exception türü (ör: System.ArgumentException).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Exception mesajı.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Numeric HResult değeri.
    /// </summary>
    public int? HResult { get; set; }

    /// <summary>
    /// Hatanın kaynak assembly veya bileşeni.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Hatanın oluştuğu hedef method.
    /// </summary>
    public string? TargetSite { get; set; }

    /// <summary>
    /// Kısaltılmış stack trace bilgisi.
    /// </summary>
    public string? Stack { get; set; }

    /// <summary>
    /// İç içe (inner) exception sayısı.
    /// </summary>
    public int? InnerCount { get; set; }
}
