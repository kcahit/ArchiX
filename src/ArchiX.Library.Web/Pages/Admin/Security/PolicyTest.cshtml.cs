using System.Security.Claims;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.ViewModels.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PolicyTestModel : PageModel
{
    private readonly IPasswordPolicyAdminService _adminService;
    private readonly IPasswordPolicyProvider _policyProvider;

    public PolicyTestModel(
        IPasswordPolicyAdminService adminService,
        IPasswordPolicyProvider policyProvider)
    {
        _adminService = adminService;
        _policyProvider = policyProvider;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    [BindProperty]
    public PolicyTestViewModel Form { get; set; } = new();

    public PasswordPolicyOptions? ActivePolicy { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        ActivePolicy = await _policyProvider.GetAsync(ApplicationId, ct).ConfigureAwait(false);
        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        ActivePolicy = await _policyProvider.GetAsync(ApplicationId, ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(Form.Password))
        {
            ModelState.AddModelError("Form.Password", "Parola boþ olamaz.");
            return Page();
        }

        var result = await _adminService.ValidatePasswordAsync(Form.Password, GetUserId(), ApplicationId, ct).ConfigureAwait(false);

        Form.Result = new PolicyTestResponseViewModel
        {
            IsValid = result.IsValid,
            Errors = result.Errors,
            StrengthScore = result.StrengthScore,
            HistoryCheckResult = result.HistoryCheckResult,
            PwnedCount = result.PwnedCount
        };

        return Page();
    }

    private static int NormalizeAppId(int value) => value > 0 ? value : 1;

    private int GetUserId()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;
        }

        return 0;
    }
}
