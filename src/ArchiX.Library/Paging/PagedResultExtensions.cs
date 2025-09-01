using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Paging;

/// <summary>
/// <see cref="PagedResult{T}"/> üretmek için extension metotlarını içerir.
/// </summary>
public static class PagedResultExtensions
{
    /// <summary>
    /// IEnumerable kaynak üzerinden sayfalama yaparak <see cref="PagedResult{T}"/> oluşturur.
    /// </summary>
    /// <typeparam name="T">Sonuç öğelerinin tipi.</typeparam>
    /// <param name="source">Kaynak koleksiyon.</param>
    /// <param name="pageNumber">Geçerli sayfa numarası (1 tabanlı).</param>
    /// <param name="pageSize">Sayfa başına öğe sayısı.</param>
    /// <returns>Sayfalama bilgisiyle birlikte <see cref="PagedResult{T}"/> nesnesi.</returns>
    public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// IQueryable kaynak üzerinden asenkron sayfalama yaparak <see cref="PagedResult{T}"/> oluşturur.
    /// EF Core sorguları için uygundur.
    /// </summary>
    /// <typeparam name="T">Sonuç öğelerinin tipi.</typeparam>
    /// <param name="query">Kaynak sorgu.</param>
    /// <param name="pageNumber">Geçerli sayfa numarası (1 tabanlı).</param>
    /// <param name="pageSize">Sayfa başına öğe sayısı.</param>
    /// <param name="cancellationToken">İptal tokenı.</param>
    /// <returns>Sayfalama bilgisiyle birlikte <see cref="PagedResult{T}"/> nesnesi.</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
