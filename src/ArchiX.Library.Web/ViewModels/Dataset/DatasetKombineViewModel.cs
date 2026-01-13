using ArchiX.Library.Web.ViewModels.Grid;

namespace ArchiX.Library.Web.ViewModels.Dataset;

public sealed class DatasetKombineViewModel
{
    public string InstanceId { get; set; } = "dskombine";

    public IReadOnlyList<ReportDatasetOptionViewModel> DatasetOptions { get; set; } = [];
    public int? SelectedReportDatasetId { get; set; }

    public IReadOnlyList<GridColumnDefinition> Columns { get; set; } = [];

    public IEnumerable<IDictionary<string, object?>> Rows { get; set; } = [];

    public int TotalRecords { get; set; }

    public string RunReportEndpoint { get; set; } = "/Tools/Dataset/Kombine?handler=Run";
}
