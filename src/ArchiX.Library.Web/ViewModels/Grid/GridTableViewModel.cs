using System.Collections.Generic;

namespace ArchiX.Library.Web.ViewModels.Grid;

public class GridTableViewModel
{
    public string Id { get; set; } = "gridTable";
    public string Title { get; set; } = string.Empty;
    public string SearchPlaceholder { get; set; } = "Genel arama...";
    public string ResetText { get; set; } = "Sıfırla";
    public string AdvancedSearchText { get; set; } = "Gelişmiş Arama";
    public string ExportText { get; set; } = "Aktar";
    public bool ShowActions { get; set; } = true;
    public IReadOnlyList<GridColumnDefinition> Columns { get; set; } = new List<GridColumnDefinition>();
    public IEnumerable<IDictionary<string, object?>> Rows { get; set; } = new List<IDictionary<string, object?>>();
}
