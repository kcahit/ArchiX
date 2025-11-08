namespace ArchiX.Library.Abstractions.Paging;

/// <summary>
/// Sayfalama sonucunu temsil eden generic arayüz.
/// </summary>
/// <typeparam name="T">Sonuç öðelerinin tipi.</typeparam>
public interface IPagedResult<T>
{
 IEnumerable<T> Items { get; }
 int TotalCount { get; }
 int PageNumber { get; }
 int PageSize { get; }
 int TotalPages { get; }
}
