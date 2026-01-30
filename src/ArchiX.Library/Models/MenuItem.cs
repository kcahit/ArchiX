namespace ArchiX.Library.Models
{
    /// <summary>Menü render modeli.</summary>
    public sealed class MenuItem
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Url { get; init; }
        public int SortOrder { get; init; }
        public int? ParentId { get; init; }
        public string? Icon { get; init; }
    }
}
