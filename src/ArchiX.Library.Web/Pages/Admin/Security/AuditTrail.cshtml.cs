using System.ComponentModel.DataAnnotations;

using ArchiX.Library.Abstractions.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AuditTrailModel : PageModel
{
    private readonly IPasswordPolicyAdminService _adminService;

    public AuditTrailModel(IPasswordPolicyAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SelectedAuditId { get; set; }

    public IReadOnlyList<PasswordPolicyAuditDto> AuditEntries { get; private set; } = Array.Empty<PasswordPolicyAuditDto>();

    public AuditDiffDto? SelectedDiff { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        var (fromFilter, toFilter) = NormalizeRange(From, To);

        AuditEntries = await _adminService.GetAuditTrailAsync(ApplicationId, fromFilter, toFilter, ct).ConfigureAwait(false);

        if (SelectedAuditId is > 0)
            SelectedDiff = await _adminService.GetAuditDiffAsync(SelectedAuditId.Value, ct).ConfigureAwait(false);

        return Page();
    }

    private static int NormalizeAppId(int value) => value > 0 ? value : 1;

    private static (DateTimeOffset?, DateTimeOffset?) NormalizeRange(DateTime? from, DateTime? to)
    {
        DateTimeOffset? fromValue = from.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc))
            : null;

        DateTimeOffset? toValue = to.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(to.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc))
            : null;

        return (fromValue, toValue);
    }
}
