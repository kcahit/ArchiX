namespace ArchiX.Library.Paging;

/// <summary>
/// Sayfalama sonucunu temsil eden generic arayüz.
/// </summary>
/// <typeparam name="T">Sonuç öğelerinin tipi.</typeparam>
public interface IPagedResult<T>
{
    /// <summary>
    /// Geçerli sayfadaki öğeler.
    /// </summary>
    IEnumerable<T> Items { get; }

    /// <summary>
    /// Tüm sonuçların toplam adedi.
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Geçerli sayfa numarası (1 tabanlı).
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// Sayfa başına öğe sayısı.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Toplam sayfa adedi.
    /// </summary>
    int TotalPages { get; }
}
