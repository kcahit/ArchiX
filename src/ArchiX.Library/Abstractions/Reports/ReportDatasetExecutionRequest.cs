namespace ArchiX.Library.Abstractions.Reports;

public sealed record ReportDatasetExecutionRequest(
    int ReportDatasetId,
    int? MaxCells = null,
    int? HardMaxRows = null,
    int? HardMaxCols = null,
    int? MaxCellChars = null,
    IReadOnlyDictionary<string, string?>? Parameters = null);
