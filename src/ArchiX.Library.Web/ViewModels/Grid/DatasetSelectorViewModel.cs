namespace ArchiX.Library.Web.ViewModels.Grid;

public enum DatasetSelectorMode
{
    GridMultiRow = 0,
    FormSingleRow = 1
}

public sealed class DatasetSelectorViewModel
{
    public string Id { get; set; } = "gridTable";

    public bool IsVisible { get; set; } = false;

    public IReadOnlyList<ReportDatasetOptionViewModel> Options { get; set; } = [];

    public int? SelectedReportDatasetId { get; set; }

    public string? RunEndpoint { get; set; }

    public string RunText { get; set; } = "Raporla";

    public string Placeholder { get; set; } = "Rapor seçin...";

    public DatasetSelectorMode Mode { get; set; } = DatasetSelectorMode.GridMultiRow;
}