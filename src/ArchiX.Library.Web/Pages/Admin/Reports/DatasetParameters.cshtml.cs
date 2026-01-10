using System.Text.Json;
using System.Text.RegularExpressions;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Formatting;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Web.Pages.Admin.Reports;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[AutoValidateAntiforgeryToken]
public sealed partial class DatasetParametersModel : PageModel
{
    private const int ParameterJsonMaxLen = 2000;

    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public DatasetParametersModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [BindProperty(SupportsGet = true)]
    public int ReportDatasetId { get; set; }

    [BindProperty]
    public FormModel Form { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public string HelpJsonExample { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        HelpJsonExample = BuildExampleJson();

        if (ReportDatasetId <= 0)
        {
            StatusMessage = "Dataset seçilmedi. Lütfen URL'e dataset id ekleyin. Örn: /Admin/Reports/DatasetParameters/1";
            return Page();
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var ds = await db.ReportDatasets
            .AsNoTracking()
            .Include(x => x.Type)
            .ThenInclude(t => t.Group)
            .SingleOrDefaultAsync(x => x.Id == ReportDatasetId, ct)
            .ConfigureAwait(false);

        if (ds is null)
            return NotFound();

        Form.DisplayName = ds.DisplayName;
        Form.Source = ds.Source;
        Form.TypeGroup = ds.Type.Group.Code;
        Form.TypeCode = ds.Type.Code;

        Form.InputParameterJson = ds.InputParameter;
        Form.OutputParameterJson = ds.OutputParameter;

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        HelpJsonExample = BuildExampleJson();

        if (ReportDatasetId <= 0)
        {
            StatusMessage = "Dataset seçilmedi. Kaydetmek için önce dataset seçin.";
            return Page();
        }

        ValidateMaxLen(nameof(Form.InputParameterJson), Form.InputParameterJson);
        ValidateMaxLen(nameof(Form.OutputParameterJson), Form.OutputParameterJson);

        ValidateJsonAndSchema(nameof(Form.InputParameterJson), Form.InputParameterJson, allowEmpty: true);
        ValidateJsonAndSchema(nameof(Form.OutputParameterJson), Form.OutputParameterJson, allowEmpty: true);

        if (!ModelState.IsValid)
            return Page();

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var ds = await db.ReportDatasets
            .SingleOrDefaultAsync(x => x.Id == ReportDatasetId, ct)
            .ConfigureAwait(false);

        if (ds is null)
            return NotFound();

        ds.InputParameter = NormalizeEmptyToNull(Form.InputParameterJson);
        ds.OutputParameter = NormalizeEmptyToNull(Form.OutputParameterJson);

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        StatusMessage = "Kaydedildi.";
        return RedirectToPage(new { reportDatasetId = ReportDatasetId });
    }

    public IActionResult OnPostValidateAsync()
    {
        HelpJsonExample = BuildExampleJson();

        if (ReportDatasetId <= 0)
        {
            StatusMessage = "Dataset seçilmedi. Doğrulamak için önce dataset seçin.";
            return Page();
        }

        ValidateMaxLen(nameof(Form.InputParameterJson), Form.InputParameterJson);
        ValidateMaxLen(nameof(Form.OutputParameterJson), Form.OutputParameterJson);

        ValidateJsonAndSchema(nameof(Form.InputParameterJson), Form.InputParameterJson, allowEmpty: true);
        ValidateJsonAndSchema(nameof(Form.OutputParameterJson), Form.OutputParameterJson, allowEmpty: true);

        if (ModelState.IsValid)
            StatusMessage = "JSON geçerli.";

        return Page();
    }

    private static string? NormalizeEmptyToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;

    private void ValidateMaxLen(string fieldName, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (value.Length > ParameterJsonMaxLen)
            ModelState.AddModelError(fieldName, $"JSON uzunluğu {ParameterJsonMaxLen} karakteri geçemez.");
    }

    private void ValidateJsonAndSchema(string fieldName, string? rawJson, bool allowEmpty)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            if (!allowEmpty)
                ModelState.AddModelError(fieldName, "JSON boş olamaz.");
            return;
        }

        if (!JsonTextFormatter.TryValidate(rawJson, out var jsonError))
        {
            ModelState.AddModelError(fieldName, $"Geçersiz JSON: {jsonError}");
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                ModelState.AddModelError(fieldName, "JSON root bir array olmalıdır. Örn: [ { \"name\": \"@P1\", \"type\": \"Int\" } ]");
                return;
            }

            var idx = 0;
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                idx++;

                if (item.ValueKind != JsonValueKind.Object)
                {
                    ModelState.AddModelError(fieldName, $"Eleman #{idx}: object olmalıdır.");
                    continue;
                }

                if (!item.TryGetProperty("name", out var nameEl) || nameEl.ValueKind != JsonValueKind.String)
                {
                    ModelState.AddModelError(fieldName, $"Eleman #{idx}: 'name' zorunludur (string).");
                    continue;
                }

                if (!item.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != JsonValueKind.String)
                {
                    ModelState.AddModelError(fieldName, $"Eleman #{idx}: 'type' zorunludur (string).");
                    continue;
                }

                var name = nameEl.GetString() ?? string.Empty;
                var type = typeEl.GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(name) || !ParamNameRegex().IsMatch(name))
                    ModelState.AddModelError(fieldName, $"Eleman #{idx}: geçersiz 'name' değeri: {name}");

                if (!IsAllowedType(type, out var typeError))
                    ModelState.AddModelError(fieldName, $"Eleman #{idx}: geçersiz 'type' değeri: {type}. {typeError}");
            }
        }
        catch (JsonException ex)
        {
            ModelState.AddModelError(fieldName, $"Geçersiz JSON: {ex.Message}");
        }
    }

    private static bool IsAllowedType(string type, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(type))
        {
            error = "type boş olamaz.";
            return false;
        }

        var t = type.Trim();

        if (t.Equals("Int", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("BigInt", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("SmallInt", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("TinyInt", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("Bit", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("Date", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("DateTime", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("DateTime2", StringComparison.OrdinalIgnoreCase) ||
            t.Equals("UniqueIdentifier", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var m = NVarCharRegex().Match(t);
        if (m.Success)
        {
            var len = m.Groups["len"].Value;
            if (len.Equals("Max", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!int.TryParse(len, out var n))
            {
                error = "NVarchar(len) parse edilemedi.";
                return false;
            }

            if (n <= 0 || n > 500)
            {
                error = "NVarchar(n) için n 1..500 aralığında olmalıdır.";
                return false;
            }

            return true;
        }

        var md = DecimalRegex().Match(t);
        if (md.Success)
        {
            if (!int.TryParse(md.Groups["p"].Value, out var p) ||
                !int.TryParse(md.Groups["s"].Value, out var s))
            {
                error = "Decimal(p,s) parse edilemedi.";
                return false;
            }

            if (p <= 0 || p > 38)
            {
                error = "Decimal(p,s) için p 1..38 aralığında olmalıdır.";
                return false;
            }

            if (s < 0 || s > p)
            {
                error = "Decimal(p,s) için s 0..p aralığında olmalıdır.";
                return false;
            }

            return true;
        }

        error = "Desteklenen tipler: NVarchar(n), NVarchar(Max), Int, BigInt, SmallInt, TinyInt, Decimal(p,s), Bit, Date, DateTime, DateTime2, UniqueIdentifier.";
        return false;
    }

    private static string BuildExampleJson()
    {
        return """
        [
          { "name": "@StartDate", "type": "DateTime" },
          { "name": "@CustomerCode", "type": "NVarchar(50)" },
          { "name": "@Amount", "type": "Decimal(18,6)" }
        ]
        """;
    }

    [GeneratedRegex(@"^@?[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ParamNameRegex();

    [GeneratedRegex(@"^NVarchar\((?<len>\d{1,4}|Max)\)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex NVarCharRegex();

    [GeneratedRegex(@"^Decimal\((?<p>\d{1,2}),(?<s>\d{1,2})\)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex DecimalRegex();

    public sealed class FormModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string TypeGroup { get; set; } = string.Empty;
        public string TypeCode { get; set; } = string.Empty;

        public string? InputParameterJson { get; set; }
        public string? OutputParameterJson { get; set; }
    }
}
