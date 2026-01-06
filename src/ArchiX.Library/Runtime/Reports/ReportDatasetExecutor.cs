using System.Data;

using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Connections;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Reports;

internal sealed class ReportDatasetExecutor(
    IDbContextFactory<AppDbContext> dbFactory,
    ConnectionStringBuilderService connectionStrings,
    ReportDatasetLimitGuard limits)
    : IReportDatasetExecutor
{
    public async Task<ReportDatasetExecutionResult> ExecuteAsync(ReportDatasetExecutionRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var dataset = await db.ReportDatasets.AsNoTracking()
            .Include(x => x.Type)
            .ThenInclude(t => t.Group)
            .SingleAsync(x => x.Id == request.ReportDatasetId, ct)
            .ConfigureAwait(false);

        if (dataset.StatusId != BaseEntity.ApprovedStatusId)
            throw new InvalidOperationException("Dataset is not approved.");

        var resolvedLimits = limits.Resolve(new ReportDatasetLimitGuard.Limits(
            MaxCells: request.MaxCells ?? 0,
            HardMaxRows: request.HardMaxRows ?? 0,
            HardMaxCols: request.HardMaxCols ?? 0,
            MaxCellChars: request.MaxCellChars ?? 0));

        var group = dataset.Type.Group.Code;
        return group switch
        {
            "Db" => await ExecuteDbAsync(dataset, resolvedLimits, ct).ConfigureAwait(false),
            "File" => await ExecuteFileAsync(dataset, resolvedLimits, ct).ConfigureAwait(false),
            _ => throw new NotSupportedException($"Dataset source group not supported: '{group}'.")
        };
    }

    private async Task<ReportDatasetExecutionResult> ExecuteDbAsync(ReportDataset dataset, ReportDatasetLimitGuard.Limits l, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dataset.ConnectionName))
            throw new InvalidOperationException("DB dataset requires ConnectionName.");

        var typeCode = dataset.Type.Code;
        if (typeCode is not ("sp" or "view" or "table"))
            throw new NotSupportedException($"DB dataset type not supported: '{typeCode}'.");

        var cs = await connectionStrings.BuildAndValidateAsync(dataset.ConnectionName!, ct).ConfigureAwait(false);

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var cmd = conn.CreateCommand();
        cmd.CommandTimeout = 60;

        if (typeCode == "sp")
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = dataset.FileName;
        }
        else
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = typeCode == "view"
                ? $"SELECT * FROM {Bracket(dataset.FileName)}"
                : $"SELECT * FROM {Bracket(dataset.FileName)}";
        }

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct).ConfigureAwait(false);

        var colCount = reader.FieldCount;
        colCount = Math.Min(colCount, l.HardMaxCols);

        var cols = new List<string>(colCount);
        for (var i = 0; i < colCount; i++)
            cols.Add(reader.GetName(i));

        var allowedRows = limits.AllowedRowsFor(colCount, l);

        var rows = new List<IReadOnlyList<object?>>(Math.Min(allowedRows, 1024));
        int r = 0;

        while (r < allowedRows && await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var row = new object?[colCount];
            for (var i = 0; i < colCount; i++)
            {
                object? v = reader.IsDBNull(i) ? null : reader.GetValue(i);
                if (v is string s)
                    v = limits.ClampCell(s, l);
                row[i] = v;
            }

            rows.Add(row);
            r++;
        }

        return new ReportDatasetExecutionResult(cols, rows);
    }

    private async Task<ReportDatasetExecutionResult> ExecuteFileAsync(ReportDataset dataset, ReportDatasetLimitGuard.Limits l, CancellationToken ct)
    {
        var typeCode = dataset.Type.Code;
        if (typeCode is not ("json" or "ndjson" or "csv" or "txt" or "xml" or "xls" or "xlsx"))
            throw new NotSupportedException($"File dataset type not supported: '{typeCode}'.");

        var root = await ReportDatasetParameterReader.GetValueAsync(dbFactory, "Reports", "FileDatasetRoot", ct).ConfigureAwait(false);
        var path = ReportDatasetFilePathResolver.ResolveAndValidate(root ?? string.Empty, dataset.SubPath, dataset.FileName);

        if (!File.Exists(path))
            throw new FileNotFoundException("Dataset file not found.", path);

        // Minimal implementation for İş #3: only NDJSON supported as rows of objects.
        if (typeCode != "ndjson")
            throw new NotSupportedException("Only 'ndjson' file datasets are supported in this iteration.");

        var cols = new List<string>();
        var rows = new List<IReadOnlyList<object?>>();

        using var fs = File.OpenRead(path);
        using var sr = new StreamReader(fs);

        string? line;
        int colCount = 0;
        int allowedRows = l.HardMaxRows;
        int r = 0;

        while (r < allowedRows && (line = await sr.ReadLineAsync(ct).ConfigureAwait(false)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var doc = System.Text.Json.JsonDocument.Parse(line);
            if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
                continue;

            if (cols.Count == 0)
            {
                foreach (var p in doc.RootElement.EnumerateObject())
                    cols.Add(p.Name);

                colCount = Math.Min(cols.Count, l.HardMaxCols);
                if (cols.Count > colCount) cols = cols.Take(colCount).ToList();

                allowedRows = limits.AllowedRowsFor(colCount, l);
            }

            var row = new object?[colCount];
            for (var i = 0; i < colCount; i++)
            {
                var name = cols[i];
                if (!doc.RootElement.TryGetProperty(name, out var el))
                {
                    row[i] = null;
                    continue;
                }

                object? v = el.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => limits.ClampCell(el.GetString(), l),
                    System.Text.Json.JsonValueKind.Number => el.TryGetInt64(out var lnum) ? lnum : el.GetDouble(),
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Null => null,
                    _ => limits.ClampCell(el.ToString(), l)
                };

                row[i] = v;
            }

            rows.Add(row);
            r++;
        }

        return new ReportDatasetExecutionResult(cols, rows);
    }

    private static string Bracket(string name)
    {
        // Very small hardening: bracket each part split by '.'
        var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join('.', parts.Select(p => $"[{p.Replace("]", "]]", StringComparison.Ordinal)}]"));
    }
}
