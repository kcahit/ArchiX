namespace ArchiX.Library.Web.ViewModels.Grid;

public class GridToolbarViewModel
{
    public string Id { get; set; } = "gridTable";
    public int TotalRecords { get; set; } = 0;
    public string SearchPlaceholder { get; set; } = "Genel arama...";
    public string ResetText { get; set; } = "Sıfırla";
    public string AdvancedSearchText { get; set; } = "Gelişmiş Arama";
    public string ExportText { get; set; } = "Aktar";
    public bool ShowAdvancedSearch { get; set; } = true;
    public bool ShowExport { get; set; } = true;
}
