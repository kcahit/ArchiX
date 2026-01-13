using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.Services.Grid;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Pages.Tools.Dataset;

public sealed class DatasetRecordPageModel : PageModel
{
    private readonly IReportDatasetExecutor _executor;
    private readonly IReportDatasetOptionService _optionsSvc;

    public DatasetRecordPageModel(IReportDatasetExecutor executor, IReportDatasetOptionService optionsSvc)
    {
        _executor = executor;
        _optionsSvc = optionsSvc;
    }

    public CustomerViewModel? Customer { get; private set; }

    public IReadOnlyList<ReportDatasetOptionViewModel> DatasetOptions { get; private set; } = [];
    public int? SelectedReportDatasetId { get; private set; }

    // Record ekranında selector daima gizli.
    public bool IsDatasetSelectorVisible { get; private set; } = false;

    // Issue #36 parametreleri (default=0)
    public bool HasRecordOperations { get; private set; } = false;

    // Yeni kayıt modu: rowId yoksa new.
    public bool IsNew { get; private set; } = false;

    // Grid state dönüş linki
    public string? ReturnContext { get; private set; }
    public string BackToGridUrl { get; private set; } = "/Tools/Dataset/Grid";

    public async Task OnGetAsync(
        [FromQuery] int? reportDatasetId,
        [FromQuery] string? rowId,
        [FromQuery] string? returnContext,
        [FromQuery] int? hasRecordOperations,
        CancellationToken ct)
    {
        DatasetOptions = await _optionsSvc.GetApprovedOptionsAsync(ct);

        ReturnContext = returnContext;
        BackToGridUrl = BuildBackUrl(returnContext);

        HasRecordOperations = (hasRecordOperations ?? 0) == 1;
        IsNew = string.IsNullOrWhiteSpace(rowId);

        SelectedReportDatasetId = (reportDatasetId.HasValue && reportDatasetId.Value > 0) ? reportDatasetId.Value : null;

        LoadFakeCustomer();
    }

    public async Task<IActionResult> OnPostRunAsync(
        [FromForm] int reportDatasetId,
        [FromQuery] string? rowId,
        [FromQuery] string? returnContext,
        CancellationToken ct)
    {
        ReturnContext = returnContext;
        BackToGridUrl = BuildBackUrl(returnContext);

        if (reportDatasetId <= 0)
            return new BadRequestResult();

        DatasetOptions = await _optionsSvc.GetApprovedOptionsAsync(ct);
        if (!DatasetOptions.Any(x => x.Id == reportDatasetId))
            return new BadRequestResult();

        SelectedReportDatasetId = reportDatasetId;

        try
        {
            var result = await _executor.ExecuteAsync(new ReportDatasetExecutionRequest(reportDatasetId), ct);

            if (result.RowCount != 1)
                return new BadRequestResult();

            var row = result.Rows[0];

            string? GetString(string name)
            {
                var idx = result.Columns
                    .Select((c, i) => (c, i))
                    .FirstOrDefault(x => string.Equals(x.c, name, StringComparison.OrdinalIgnoreCase))
                    .i;

                if (idx < 0 || idx >= row.Count) return null;
                return row[idx]?.ToString();
            }

            int? GetInt(string name)
            {
                var s = GetString(name);
                if (int.TryParse(s, out var v)) return v;
                return null;
            }

            Customer = new CustomerViewModel(
                Id: GetInt("id"),
                Name: GetString("name"),
                Email: GetString("email"),
                Phone: GetString("phone"),
                City: GetString("city"),
                TaxNumber: GetString("taxNumber"),
                Address: GetString("address"),
                Status: GetString("status"));

            return new OkResult();
        }
        catch
        {
            return new BadRequestResult();
        }
    }

    public IActionResult OnPostUpdate([FromQuery] int? hasRecordOperations)
    {
        HasRecordOperations = (hasRecordOperations ?? 0) == 1;

        if (!HasRecordOperations)
            return new BadRequestResult();

        return new OkResult();
    }

    public IActionResult OnPostDelete([FromQuery] int? hasRecordOperations, [FromQuery] string? rowId)
    {
        HasRecordOperations = (hasRecordOperations ?? 0) == 1;
        IsNew = string.IsNullOrWhiteSpace(rowId);

        if (!HasRecordOperations)
            return new BadRequestResult();

        if (IsNew)
            return new BadRequestResult();

        return new OkResult();
    }

    private static string BuildBackUrl(string? returnContext)
    {
        if (!GridReturnContextCodec.TryDecode(returnContext, out _))
            return "/Tools/Dataset/Grid";

        return "/Tools/Dataset/Grid?returnContext=" + Uri.EscapeDataString(returnContext!);
    }

    private void LoadFakeCustomer()
    {
        Customer = new CustomerViewModel(
            Id: 1,
            Name: "Musteri 1",
            Email: "musteri1@example.com",
            Phone: "0532 000 0001",
            City: "Istanbul",
            TaxNumber: "1234567890",
            Address: "Ornek Mah. Ornek Sok. No:1",
            Status: "Aktif");
    }

    public sealed record CustomerViewModel(
        int? Id,
        string? Name,
        string? Email,
        string? Phone,
        string? City,
        string? TaxNumber,
        string? Address,
        string? Status);
}
