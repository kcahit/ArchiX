using Microsoft.Extensions.Options;

namespace ArchiX.Library.Runtime.Reports;

internal sealed class ReportDatasetLimitGuard(IOptions<ReportDatasetLimitOptions> options)
{
    private readonly ReportDatasetLimitOptions _global = options.Value;

    public Limits Resolve(Limits? datasetOrRequest)
    {
        if (datasetOrRequest is null)
            return new Limits(_global.MaxCells, _global.HardMaxRows, _global.HardMaxCols, _global.MaxCellChars);

        return new Limits(
            MaxCells: MinOrGlobal(datasetOrRequest.MaxCells, _global.MaxCells),
            HardMaxRows: MinOrGlobal(datasetOrRequest.HardMaxRows, _global.HardMaxRows),
            HardMaxCols: MinOrGlobal(datasetOrRequest.HardMaxCols, _global.HardMaxCols),
            MaxCellChars: MinOrGlobal(datasetOrRequest.MaxCellChars, _global.MaxCellChars));

        static int MinOrGlobal(int? requested, int global)
        {
            if (requested is null || requested.Value <= 0) return global;
            return Math.Min(requested.Value, global);
        }
    }

    public sealed record Limits(int MaxCells, int HardMaxRows, int HardMaxCols, int MaxCellChars);

    public int AllowedRowsFor(int colCount, Limits l)
    {
        if (colCount <= 0) return 0;

        var safeCols = Math.Min(colCount, l.HardMaxCols);
        if (safeCols <= 0) return 0;

        var byCells = l.MaxCells / safeCols;
        return Math.Max(0, Math.Min(l.HardMaxRows, byCells));
    }

    public string? ClampCell(string? value, Limits l)
    {
        if (value is null) return null;
        if (l.MaxCellChars <= 0) return string.Empty;
        if (value.Length <= l.MaxCellChars) return value;
        return value[..l.MaxCellChars];
    }
}
