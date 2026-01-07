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

    public void OnGet()
    {
        LoadFakeCustomer();
    }

    public async Task<IActionResult> OnPostRunAsync([FromQuery] int reportDatasetId, CancellationToken ct)
    {
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

    private void LoadFakeCustomer()
    {
        Customer = new CustomerViewModel(
            Id: 1,
            Name: "Müşteri 1",
            Email: "musteri1@example.com",
            Phone: "0532 000 0001",
            City: "Istanbul",
            TaxNumber: "1234567890",
            Address: "Örnek Mah. Örnek Sok. No:1",
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
