namespace ArchiX.Library.Abstractions.Security;

/// <summary>
/// Parola doğrulama hata mesajlarını yerelleştirme servisi.
/// </summary>
public interface IPasswordValidationMessageProvider
{
    /// <summary>
    /// Hata kodunu yerelleştirilmiş mesaja çevirir.
    /// </summary>
    /// <param name="errorCode">Hata kodu (örn: MIN_LENGTH, REQ_UPPER)</param>
    /// <param name="args">Mesaj parametreleri (örn: minimum uzunluk değeri)</param>
    /// <returns>Yerelleştirilmiş hata mesajı</returns>
    string GetMessage(string errorCode, params object[] args);

    /// <summary>
    /// Birden fazla hata kodunu yerelleştirilmiş mesajlara çevirir.
    /// </summary>
    /// <param name="errorCodes">Hata kodları listesi</param>
    /// <returns>Yerelleştirilmiş hata mesajları</returns>
    IReadOnlyList<string> GetMessages(IEnumerable<string> errorCodes);

    /// <summary>
    /// Mevcut kültürü değiştirir.
    /// </summary>
    /// <param name="cultureName">Kültür adı (tr-TR, en-US)</param>
    void SetCulture(string cultureName);
}
