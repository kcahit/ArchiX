using ArchiX.Library.Abstractions.Reports;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Templates.Modern.Pages.Raporlar;

public sealed class FormModel : PageModel
{
    private readonly IReportDatasetExecutor _executor;

    public FormModel(IReportDatasetExecutor executor)
    {
        _executor = executor;
    }

    public CustomerViewModel? Customer { get; private set; }

    // Issue #36 parametreleri (default=0)
    public bool HasRecordOperations { get; private set; } = false;

    // Yeni kayıt modu (Karar-3/1.2.8)
    public bool IsNew { get; private set; } = false;

    // Grid state dönüş linki
    public string? ReturnContext { get; private set; }
    public string BackToGridUrl { get; private set; } = "/Raporlar/GridListe";

    public void OnGet([FromQuery] string? returnContext, [FromQuery] int? hasRecordOperations, [FromQuery] string? mode)
    {
        ReturnContext = returnContext;
        BackToGridUrl = BuildBackUrl(returnContext);

        HasRecordOperations = (hasRecordOperations ?? 0) == 1;
        IsNew = string.Equals(mode, "new", StringComparison.OrdinalIgnoreCase);

        LoadFakeCustomer();
    }

    public async Task<IActionResult> OnPostRunAsync([FromQuery] int reportDatasetId, [FromQuery] string? returnContext, CancellationToken ct)
    {
        ReturnContext = returnContext;
        BackToGridUrl = BuildBackUrl(returnContext);

        if (reportDatasetId <= 0)
            return new BadRequestResult();

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

    // Issue #36 / 1.2.4: Backend enforce
    public IActionResult OnPostUpdate([FromQuery] int? hasRecordOperations)
    {
        HasRecordOperations = (hasRecordOperations ?? 0) == 1;

        if (!HasRecordOperations)
            return new BadRequestResult();

        // Şimdilik fake update
        return new OkResult();
    }

    // Issue #36 / 1.2.4 + 1.2.8: Backend enforce (new modda silme yok)
    public IActionResult OnPostDelete([FromQuery] int? hasRecordOperations, [FromQuery] string? mode)
    {
        HasRecordOperations = (hasRecordOperations ?? 0) == 1;
        IsNew = string.Equals(mode, "new", StringComparison.OrdinalIgnoreCase);

        if (!HasRecordOperations)
            return new BadRequestResult();

        if (IsNew)
            return new BadRequestResult();

        // Şimdilik fake delete
        return new OkResult();
    }

    private static string BuildBackUrl(string? returnContext)
    {
        if (string.IsNullOrWhiteSpace(returnContext))
            return "/Raporlar/GridListe";

        return "/Raporlar/GridListe?returnContext=" + Uri.EscapeDataString(returnContext);
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
