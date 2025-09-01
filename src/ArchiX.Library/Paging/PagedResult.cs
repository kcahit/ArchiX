namespace ArchiX.Library.Paging;

/// <summary>
/// <see cref="IPagedResult{T}"/> arayüzünün varsayılan implementasyonu.
/// Sayfalama yapılmış veri setini temsil eder.
/// </summary>
/// <typeparam name="T">Sonuç öğelerinin tipi.</typeparam>
public class PagedResult<T> : IPagedResult<T>
{
    /// <summary>
    /// Geçerli sayfadaki öğeler.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Tüm sonuçların toplam adedi.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Geçerli sayfa numarası (1 tabanlı).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Sayfa başına öğe sayısı.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Toplam sayfa adedi.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Yeni bir <see cref="PagedResult{T}"/> oluşturur.
    /// </summary>
    /// <param name="items">Geçerli sayfadaki öğeler.</param>
    /// <param name="totalCount">Tüm sonuçların toplam adedi.</param>
    /// <param name="pageNumber">Geçerli sayfa numarası.</param>
    /// <param name="pageSize">Sayfa başına öğe sayısı.</param>
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
