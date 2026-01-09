using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.ViewModels.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.WebHost.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class IndexModel : PageModel
{
    private readonly IPasswordPolicyAdminService _adminService;

    public IndexModel(IPasswordPolicyAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    public SecurityDashboardViewModel Dashboard { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        var data = await _adminService.GetDashboardDataAsync(ApplicationId, ct).ConfigureAwait(false);
        Dashboard = MapToViewModel(data);
        return Page();
    }

    private static int NormalizeAppId(int value) => value > 0 ? value : 1;

    private static SecurityDashboardViewModel MapToViewModel(SecurityDashboardData data)
    {
        var recent = data.RecentChanges
            .Select(x => new RecentAuditEntry
            {
                AuditId = x.AuditId,
                ChangedAt = x.ChangedAt,
                UserDisplayName = x.UserDisplayName,
                Summary = x.Summary
            })
            .ToArray();

        return new SecurityDashboardViewModel
        {
            ActivePolicy = data.Policy,
            BlacklistWordCount = data.BlacklistWordCount,
            ExpiredPasswordCount = data.ExpiredPasswordCount,
            Last30DaysErrors = new Dictionary<string, int>(data.Last30DaysErrors, StringComparer.OrdinalIgnoreCase),
            RecentChanges = recent
        };
    }
}
