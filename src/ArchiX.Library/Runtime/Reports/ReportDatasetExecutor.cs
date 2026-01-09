using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

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
    private const int MaxSchemaJsonLen = 2000;

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
            "Db" => await ExecuteDbAsync(dataset, request, resolvedLimits, ct).ConfigureAwait(false),
            "File" => await ExecuteFileAsync(dataset, resolvedLimits, ct).ConfigureAwait(false),
            _ => throw new NotSupportedException($"Dataset source group not supported: '{group}'.")
        };
    }

    private async Task<ReportDatasetExecutionResult> ExecuteDbAsync(
        ReportDataset dataset,
        ReportDatasetExecutionRequest request,
        ReportDatasetLimitGuard.Limits l,
        CancellationToken ct)
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

            // İş #11: InputParameter JSON şeması -> SqlParameter (fail-closed)
            ApplySpParameters(dataset, request.Parameters, cmd);
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

    private static void ApplySpParameters(
        ReportDataset dataset,
        IReadOnlyDictionary<string, string?>? requestParameters,
        SqlCommand cmd)
    {
        // Şema yoksa parametre yok demektir
        if (string.IsNullOrWhiteSpace(dataset.InputParameter))
            return;

        if (dataset.InputParameter.Length > MaxSchemaJsonLen)
            throw new InvalidOperationException("InputParameter schema is too long.");

        List<ParamSchemaItem> schema;
        try
        {
            schema = JsonSerializer.Deserialize<List<ParamSchemaItem>>(dataset.InputParameter) ?? [];
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("InputParameter schema JSON is invalid.");
        }

        if (schema.Count == 0)
            return;

        var dict = requestParameters ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in schema)
        {
            if (item is null)
                throw new InvalidOperationException("InputParameter schema item is null.");

            var rawName = (item.Name ?? string.Empty).Trim();
            var rawType = (item.Type ?? string.Empty).Trim();

            if (!IsValidParamName(rawName))
                throw new InvalidOperationException($"Invalid parameter name in schema: '{rawName}'.");

            if (!TryParseAllowedType(rawType, out var sqlType, out var size, out var precision, out var scale))
                throw new InvalidOperationException($"Invalid parameter type in schema: '{rawType}'.");

            var normalizedKey = NormalizeKey(rawName); // @StartDate -> StartDate
            dict.TryGetValue(normalizedKey, out var rawValue);

            var p = new SqlParameter
            {
                ParameterName = rawName.StartsWith('@') ? rawName : "@" + rawName,
                SqlDbType = sqlType
            };

            if (sqlType == SqlDbType.NVarChar && size.HasValue)
            {
                p.Size = size.Value;
            }
            else if (sqlType == SqlDbType.Decimal && precision.HasValue && scale.HasValue)
            {
                p.Precision = precision.Value;
                p.Scale = scale.Value;
            }

            p.Value = ParseValueOrDBNull(rawValue, sqlType);

            cmd.Parameters.Add(p);
        }
    }

    private static string NormalizeKey(string paramName)
    {
        var n = paramName.Trim();
        if (n.StartsWith('@'))
            n = n[1..];
        return n;
    }

    private static bool IsValidParamName(string name)
    {
        // ^@?[A-Za-z_][A-Za-z0-9_]*$
        return Regex.IsMatch(name, @"^@?[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant);
    }

    private static bool TryParseAllowedType(
        string type,
        out SqlDbType sqlType,
        out int? size,
        out byte? precision,
        out byte? scale)
    {
        sqlType = default;
        size = null;
        precision = null;
        scale = null;

        if (string.IsNullOrWhiteSpace(type))
            return false;

        if (type.Equals("Int", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.Int;
            return true;
        }

        if (type.Equals("BigInt", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.BigInt;
            return true;
        }

        if (type.Equals("SmallInt", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.SmallInt;
            return true;
        }

        if (type.Equals("TinyInt", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.TinyInt;
            return true;
        }

        if (type.Equals("Bit", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.Bit;
            return true;
        }

        if (type.Equals("Date", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.Date;
            return true;
        }

        if (type.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.DateTime;
            return true;
        }

        if (type.Equals("DateTime2", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.DateTime2;
            return true;
        }

        if (type.Equals("UniqueIdentifier", StringComparison.OrdinalIgnoreCase))
        {
            sqlType = SqlDbType.UniqueIdentifier;
            return true;
        }

        // NVarchar(n) / NVarchar(Max)
        var nv = Regex.Match(type, @"^NVarchar\((?<len>\d{1,4}|Max)\)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        if (nv.Success)
        {
            sqlType = SqlDbType.NVarChar;

            var len = nv.Groups["len"].Value;
            if (len.Equals("Max", StringComparison.OrdinalIgnoreCase))
            {
                size = -1;
                return true;
            }

            if (!int.TryParse(len, out var n) || n < 1 || n > 500)
                return false;

            size = n;
            return true;
        }

        // Decimal(p,s)
        var dec = Regex.Match(type, @"^Decimal\((?<p>\d{1,2}),(?<s>\d{1,2})\)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        if (dec.Success)
        {
            if (!byte.TryParse(dec.Groups["p"].Value, out var p) || p < 1 || p > 38)
                return false;

            if (!byte.TryParse(dec.Groups["s"].Value, out var s) || s > p)
                return false;

            sqlType = SqlDbType.Decimal;
            precision = p;
            scale = s;
            return true;
        }

        return false;
    }

    private static object ParseValueOrDBNull(string? raw, SqlDbType sqlType)
    {
        if (raw is null)
            return DBNull.Value;

        var s = raw.Trim();
        if (s.Length == 0)
            return DBNull.Value;

        try
        {
            return sqlType switch
            {
                SqlDbType.Int => int.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                SqlDbType.BigInt => long.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                SqlDbType.SmallInt => short.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                SqlDbType.TinyInt => byte.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                SqlDbType.Bit => bool.TryParse(s, out var b)
                    ? b
                    : (s == "1" ? true : s == "0" ? false : throw new FormatException()),
                SqlDbType.Date => DateOnly.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                SqlDbType.DateTime => DateTime.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                SqlDbType.DateTime2 => DateTime.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                SqlDbType.UniqueIdentifier => Guid.Parse(s),
                SqlDbType.NVarChar => s,
                SqlDbType.Decimal => decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
                _ => throw new NotSupportedException($"SqlDbType not supported in parser: {sqlType}")
            };
        }
        catch
        {
            // fail-closed: parse edilemeyen parametre -> hata
            throw new InvalidOperationException($"Invalid value for type '{sqlType}': '{raw}'.");
        }
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

            using var doc = JsonDocument.Parse(line);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
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
                    JsonValueKind.String => limits.ClampCell(el.GetString(), l),
                    JsonValueKind.Number => el.TryGetInt64(out var lnum) ? lnum : el.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
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

    private sealed class ParamSchemaItem
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
    }
}
