namespace ArchiX.Library.Abstractions.Reports;

public interface IReportDatasetExecutor
{
    Task<ReportDatasetExecutionResult> ExecuteAsync(ReportDatasetExecutionRequest request, CancellationToken ct = default);
}
