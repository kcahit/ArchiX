using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using ArchiX.Library.Abstractions.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class BlacklistModel : PageModel
{
    private readonly IPasswordPolicyAdminService _adminService;

    public BlacklistModel(IPasswordPolicyAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    [BindProperty]
    [Display(Name = "Kelime")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Kelime 2-256 karakter aralýðýnda olmalýdýr.")]
    public string? NewWord { get; set; }

    public IReadOnlyList<PasswordBlacklistWordDto> Words { get; private set; } = Array.Empty<PasswordBlacklistWordDto>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        await LoadAsync(ct).ConfigureAwait(false);
        return Page();
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAddAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);

        if (!ModelState.IsValid)
        {
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }

        if (string.IsNullOrWhiteSpace(NewWord))
        {
            ModelState.AddModelError(nameof(NewWord), "Kelime boþ olamaz.");
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }

        var added = await _adminService.TryAddBlacklistWordAsync(ApplicationId, NewWord, GetUserId(), ct).ConfigureAwait(false);
        if (!added)
        {
            ModelState.AddModelError(nameof(NewWord), "Kelime zaten listede.");
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }

        StatusMessage = "Kelime eklendi.";
        return RedirectToPage(new { applicationId = ApplicationId });
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        if (id <= 0)
        {
            StatusMessage = "Geçersiz kayýt.";
            return RedirectToPage(new { applicationId = ApplicationId });
        }

        var removed = await _adminService.TryRemoveBlacklistWordAsync(id, GetUserId(), ct).ConfigureAwait(false);
        StatusMessage = removed ? "Kelime silindi." : "Kayýt bulunamadý.";
        return RedirectToPage(new { applicationId = ApplicationId });
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Words = await _adminService.GetBlacklistAsync(ApplicationId, ct).ConfigureAwait(false);
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
