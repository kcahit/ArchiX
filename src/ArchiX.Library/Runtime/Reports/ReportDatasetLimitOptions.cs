namespace ArchiX.Library.Runtime.Reports;

public sealed class ReportDatasetLimitOptions
{
    public int MaxCells { get; set; } = 200_000;
    public int HardMaxRows { get; set; } = 20_000;
    public int HardMaxCols { get; set; } = 200;
    public int MaxCellChars { get; set; } = 4_000;
}
