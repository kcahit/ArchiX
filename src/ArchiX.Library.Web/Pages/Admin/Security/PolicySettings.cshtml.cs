using System.Text.Json;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.ViewModels.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Web.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PolicySettingsModel : PageModel
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly IPasswordPolicyAdminService _adminService;
    private readonly ILogger<PolicySettingsModel> _logger;

    public PolicySettingsModel(
        IPasswordPolicyProvider policyProvider,
        IPasswordPolicyAdminService adminService,
        ILogger<PolicySettingsModel> logger)
    {
        _policyProvider = policyProvider;
        _adminService = adminService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    [BindProperty]
    public PolicySettingsViewModel Form { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        await LoadAsync(ct).ConfigureAwait(false);
        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Form.MinLength > Form.MaxLength)
        {
            ModelState.AddModelError(string.Empty, "Minimum uzunluk maksimumdan büyük olamaz.");
            return Page();
        }

        var options = Form.ToOptions();
        var json = JsonSerializer.Serialize(options, JsonOpts);

        try
        {
            await _adminService.UpdateAsync(json, ApplicationId, null, ct).ConfigureAwait(false);
            StatusMessage = "Parola politikasý güncellendi.";
            return RedirectToPage(new { applicationId = ApplicationId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Policy update rejected. ApplicationId={ApplicationId}", ApplicationId);
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Policy update failed. ApplicationId={ApplicationId}", ApplicationId);
            ModelState.AddModelError(string.Empty, "Güncelleme baþarýsýz: " + ex.Message);
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        var options = await _policyProvider.GetAsync(ApplicationId, ct).ConfigureAwait(false);
        Form = PolicySettingsViewModel.FromOptions(options);
    }

    private static int NormalizeAppId(int value) => value > 0 ? value : 1;
}
