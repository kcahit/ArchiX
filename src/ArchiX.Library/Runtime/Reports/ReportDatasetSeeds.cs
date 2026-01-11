using ArchiX.Library.Entities;

namespace ArchiX.Library.Runtime.Reports;

internal static class ReportDatasetSeeds
{
    internal sealed record GroupSeed(string Code, string Name, string? Description);

    internal sealed record TypeSeed(string GroupCode, string Code, string Name, string? Description);

    internal static readonly GroupSeed[] TypeGroups =
    [
        new("Db", "Database", "DB backed datasets"),
        new("File", "File", "File backed datasets"),
        new("Other", "Other", "Other sources (future)")
    ];

    internal static readonly TypeSeed[] Types =
    [
        // Db
        new("Db", "sp", "Stored Procedure", null),
        new("Db", "view", "View", null),
        new("Db", "table", "Table", null),

        // File
        new("File", "json", "JSON", null),
        new("File", "ndjson", "NDJSON", null),
        new("File", "csv", "CSV", null),
        new("File", "txt", "Text", null),
        new("File", "xml", "XML", null),
        new("File", "xls", "Excel (XLS)", null),
        new("File", "xlsx", "Excel (XLSX)", null)
    ];
}
