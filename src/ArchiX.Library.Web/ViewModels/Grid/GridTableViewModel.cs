namespace ArchiX.Library.Web.ViewModels.Grid;

public class GridTableViewModel
{
    public string? Id { get; set; }

    public string? Title { get; set; }

    public string SearchPlaceholder { get; set; } = "Genel arama...";

    public string ResetText { get; set; } = "Sıfırla";

    public string AdvancedSearchText { get; set; } = "Gelişmiş Arama";

    public string ExportText { get; set; } = "Aktar";

    public IReadOnlyList<GridColumnDefinition> Columns { get; set; } = [];

    public IEnumerable<IDictionary<string, object?>> Rows { get; set; } = [];

    public bool ShowActions { get; set; } = false;

    public bool ShowToolbar { get; set; } = true;

    public GridToolbarViewModel? Toolbar { get; set; }

    /// <summary>
    /// Entity-specific rule: IDs for which delete button should be hidden.
    /// Example: ApplicationId=1 (system record).
    /// </summary>
    public IReadOnlyList<int> HideDeleteForIds { get; set; } = [];
}
