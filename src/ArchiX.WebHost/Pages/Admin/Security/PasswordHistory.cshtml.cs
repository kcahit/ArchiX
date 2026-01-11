using System.ComponentModel.DataAnnotations;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.ViewModels.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.WebHost.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PasswordHistoryModel : PageModel
{
    private readonly IPasswordPolicyAdminService _adminService;

    public PasswordHistoryModel(IPasswordPolicyAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Kullan�c� ID")]
    public string? Query { get; set; }

    [BindProperty(SupportsGet = true)]
    [Range(1, 100, ErrorMessage = "Kay�t say�s� 1-100 aral���nda olmal�.")]
    [Display(Name = "Kay�t Say�s�")]
    public int Take { get; set; } = 20;

    public PasswordHistoryViewModel History { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);

        if (string.IsNullOrWhiteSpace(Query))
        {
            History = new PasswordHistoryViewModel { Query = null };
            return Page();
        }

        var trimmed = Query.Trim();
        if (!int.TryParse(trimmed, out var userId) || userId <= 0)
        {
            ModelState.AddModelError(nameof(Query), "Ge�erli bir kullan�c� ID girin.");
            History = new PasswordHistoryViewModel { Query = trimmed };
            return Page();
        }

        var take = Math.Clamp(Take, 1, 100);
        if (take != Take)
            Take = take;

        var entries = await _adminService.GetUserPasswordHistoryAsync(userId, take, ct).ConfigureAwait(false);
        var mapped = entries
            .Select(x => new UserPasswordHistoryViewModel
            {
                UserId = x.UserId,
                UserName = x.UserName,
                MaskedHash = x.MaskedHash,
                HashAlgorithm = x.HashAlgorithm,
                CreatedAtUtc = x.CreatedAtUtc,
                IsExpired = x.IsExpired
            })
            .ToArray();

        History = new PasswordHistoryViewModel
        {
            Query = trimmed,
            Entries = mapped
        };

        return Page();
    }

    private static int NormalizeAppId(int value) => value > 0 ? value : 1;
}
