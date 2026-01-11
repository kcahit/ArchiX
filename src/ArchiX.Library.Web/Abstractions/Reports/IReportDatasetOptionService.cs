using ArchiX.Library.Web.ViewModels.Grid;

namespace ArchiX.Library.Web.Abstractions.Reports;

public interface IReportDatasetOptionService
{
    Task<IReadOnlyList<ReportDatasetOptionViewModel>> GetApprovedOptionsAsync(CancellationToken ct = default);
}