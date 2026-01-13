using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Pages.Tools.Dataset;

public sealed class DatasetKombinePageModel : PageModel
{
    private const string ParamPrefix = "p_";

    private readonly IReportDatasetExecutor _executor;
    private readonly IReportDatasetOptionService _optionsSvc;

    public DatasetKombinePageModel(IReportDatasetExecutor executor, IReportDatasetOptionService optionsSvc)
    {
        _executor = executor;
        _optionsSvc = optionsSvc;
    }

    public IReadOnlyList<GridColumnDefinition> Columns { get; private set; } = [];

    public List<IDictionary<string, object?>> Rows { get; private set; } = [];

    public IReadOnlyList<ReportDatasetOptionViewModel> DatasetOptions { get; private set; } = [];
    public int? SelectedReportDatasetId { get; private set; }

    public List<EmployeeData> SampleData { get; private set; } = [];

    public async Task OnGetAsync([FromQuery] int? reportDatasetId, CancellationToken ct)
    {
        DatasetOptions = await _optionsSvc.GetApprovedOptionsAsync(ct);

        LoadSampleData();

        if (!reportDatasetId.HasValue || reportDatasetId.Value <= 0)
            return;

        SelectedReportDatasetId = reportDatasetId.Value;

        if (!DatasetOptions.Any(x => x.Id == reportDatasetId.Value))
            return;

        try
        {
            var result = await _executor.ExecuteAsync(new ReportDatasetExecutionRequest(reportDatasetId.Value), ct);

            Columns = result.Columns
                .Select(c => new GridColumnDefinition(c, c))
                .ToList();

            Rows = MapToRowsOrThrow(result);
        }
        catch
        {
            // page renders even on failures
        }
    }

    public async Task<IActionResult> OnPostRunAsync([FromForm] int reportDatasetId, CancellationToken ct)
    {
        var hasForm = Request?.HasFormContentType == true;

        if (reportDatasetId <= 0)
            return new BadRequestResult();

        List<ReportDatasetOptionViewModel> opts = (await _optionsSvc.GetApprovedOptionsAsync(ct)).ToList();
        if (!opts.Any(x => x.Id == reportDatasetId))
            return new BadRequestResult();

        Dictionary<string, string?> parameters = hasForm
            ? new(ExtractParametersFromForm(Request!.Form), StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            ReportDatasetExecutionResult result = await _executor.ExecuteAsync(
                new ReportDatasetExecutionRequest(reportDatasetId, Parameters: parameters),
                ct);

            Columns = result.Columns
                .Select(c => new GridColumnDefinition(c, c))
                .ToList();

            Rows = MapToRowsOrThrow(result);

            SelectedReportDatasetId = reportDatasetId;
            DatasetOptions = (await _optionsSvc.GetApprovedOptionsAsync(ct));

            return Page();
        }
        catch
        {
            return new BadRequestResult();
        }
    }

    private static Dictionary<string, string?> ExtractParametersFromForm(Microsoft.AspNetCore.Http.IFormCollection form)
    {
        Dictionary<string, string?> dict = new(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in form)
        {
            var key = kv.Key ?? string.Empty;
            if (!key.StartsWith(ParamPrefix, StringComparison.Ordinal))
                continue;

            var normalized = key[ParamPrefix.Length..].Trim();
            if (normalized.Length == 0)
                continue;

            dict[normalized] = kv.Value.Count > 0 ? kv.Value[0] : null;
        }

        return dict;
    }

    private static List<IDictionary<string, object?>> MapToRowsOrThrow(ReportDatasetExecutionResult result)
    {
        if (result.Columns.Count <= 0)
            throw new InvalidOperationException("Dataset returned no columns.");

        var rows = new List<IDictionary<string, object?>>(result.Rows.Count);

        foreach (var r in result.Rows)
        {
            if (r.Count != result.Columns.Count)
                throw new InvalidOperationException("Dataset row/column mismatch.");

            var dict = new Dictionary<string, object?>(StringComparer.Ordinal);

            for (var i = 0; i < result.Columns.Count; i++)
                dict[result.Columns[i]] = r[i];

            rows.Add(dict);
        }

        return rows;
    }

    private void LoadSampleData()
    {
        SampleData = new List<EmployeeData>
        {
            new() { Id = 1, Name = "Ahmet Yılmaz", Email = "ahmet@example.com", Phone = "0532 123 4567", Department = "IT", Salary = 15000, Experience = 5, City = "Istanbul", Position = "Yazılım Geliştirici", StartDate = "2019-03-15", Status = "Aktif" },
            new() { Id = 2, Name = "Ayşe Demir", Email = "ayse@example.com", Phone = "0533 234 5678", Department = "Satış", Salary = 12000, Experience = 3, City = "Ankara", Position = "Satış Temsilcisi", StartDate = "2021-06-20", Status = "Aktif" },
        };
    }

    public sealed class EmployeeData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Salary { get; set; }
        public int Experience { get; set; }
        public string City { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
