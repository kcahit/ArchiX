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

    public bool IncludeAdvancedSearchPanel { get; set; } = false;

    // GridListe/GridTable -> FormRecord açma davranışı (Issue #36 / 1.2.1-1.2.3)
    // Default=0: "Değiştir" aksiyonu render edilmez.
    public int IsFormOpenEnabled { get; set; } = 0;

    // Kombine / Dataset-driven destek (Issue #17 - bölüm 12)
    public bool ShowDatasetSelector { get; set; } = false;

    // Dropdown seçenekleri: sadece Approved datasetler buraya doldurulacak
    public IReadOnlyList<ReportDatasetOptionViewModel> DatasetOptions { get; set; } =
        [];

    // Seçili dataset (dropdown)
    public int? SelectedReportDatasetId { get; set; }

    // Raporla butonunun POST edeceği endpoint (ör: "/Raporlar/Kombine?handler=Run")
    public string? RunReportEndpoint { get; set; }

    public string RunReportText { get; set; } = "Raporla";
    public string DatasetPlaceholder { get; set; } = "Rapor seçin...";

    // Entity-driven record endpoint (Issue #32)
    public string? RecordEndpoint { get; set; }

    // Silinmişleri göster checkbox (Issue #32)
    public bool ShowDeletedToggle { get; set; } = false;
}
