namespace ArchiX.Library.Sorting;

/// <summary>
/// Sıralama yapılacak alan ve yön bilgisini temsil eder.
/// </summary>
public sealed class SortItem
{
    /// <summary>
    /// Sıralanacak alan adı (property veya column).
    /// </summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Sıralama yönü (Ascending veya Descending).
    /// </summary>
    public SortDirection Direction { get; init; }
}
