using ArchiX.Library.Result;

namespace ArchiX.Library.Abstractions.Reports;

public sealed record ReportDatasetExecutionResult(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows)
{
    public int RowCount => Rows.Count;
    public int ColumnCount => Columns.Count;
}
