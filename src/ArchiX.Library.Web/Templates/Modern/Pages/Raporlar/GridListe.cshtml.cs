using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Templates.Modern.Pages.Raporlar;

public class GridListeModel : PageModel
{
    private readonly IReportDatasetExecutor _executor;
    private readonly IReportDatasetOptionService _optionsSvc;

    public GridListeModel(IReportDatasetExecutor executor, IReportDatasetOptionService optionsSvc)
    {
        _executor = executor;
        _optionsSvc = optionsSvc;
    }

    public IReadOnlyList<GridColumnDefinition> Columns { get; private set; } = new List<GridColumnDefinition>();
    public IEnumerable<IDictionary<string, object?>> Rows { get; private set; } = [];

    public IReadOnlyList<ReportDatasetOptionViewModel> DatasetOptions { get; private set; } = [];
    public int? SelectedReportDatasetId { get; private set; }

    public async Task OnGetAsync([FromQuery] int? reportDatasetId, CancellationToken ct)
    {
        DatasetOptions = await _optionsSvc.GetApprovedOptionsAsync(ct);

        if (!reportDatasetId.HasValue || reportDatasetId.Value <= 0)
        {
            // İlk açılış: boş grid (fake data yok)
            Columns = [];
            Rows = [];
            return;
        }

        SelectedReportDatasetId = reportDatasetId.Value;

        // fail-closed: Approved olmayan dataset seçilirse sayfa boş kalsın.
        if (!DatasetOptions.Any(x => x.Id == reportDatasetId.Value))
        {
            Columns = [];
            Rows = [];
            return;
        }

        await TryLoadDatasetAsync(reportDatasetId.Value, parameters: null, ct);
    }

    public async Task<IActionResult> OnPostRunAsync([FromForm] int reportDatasetId, CancellationToken ct)
    {
        if (reportDatasetId <= 0)
            return new BadRequestResult();

        DatasetOptions = await _optionsSvc.GetApprovedOptionsAsync(ct);
        SelectedReportDatasetId = reportDatasetId;

        // fail-closed: ApprovedOnly kontrolü (UI bypass edilirse bile)
        if (!DatasetOptions.Any(x => x.Id == reportDatasetId))
            return new BadRequestResult();

        Dictionary<string, string?> parameters = Request?.HasFormContentType == true
            ? ExtractParametersFromForm(Request.Form)
            : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        await TryLoadDatasetAsync(reportDatasetId, parameters, ct);

        // Sayfayı refresh ederek aynı ekranda grid’i yeniden render et.
        return Page();
    }

    private async Task TryLoadDatasetAsync(int reportDatasetId, IReadOnlyDictionary<string, string?>? parameters, CancellationToken ct)
    {
        try
        {
            var result = await _executor.ExecuteAsync(
                new ReportDatasetExecutionRequest(reportDatasetId, Parameters: parameters),
                ct);

            Columns = result.Columns
                .Select(c => new GridColumnDefinition(c, c))
                .ToList();

            Rows = result.Rows
                .Select(r =>
                {
                    var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                    for (var i = 0; i < result.Columns.Count; i++)
                        dict[result.Columns[i]] = r[i];

                    return (IDictionary<string, object?>)dict;
                })
                .ToList();
        }
        catch
        {
            Columns = [];
            Rows = [];
        }
    }

    private const string ParamPrefix = "p_";

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
}
