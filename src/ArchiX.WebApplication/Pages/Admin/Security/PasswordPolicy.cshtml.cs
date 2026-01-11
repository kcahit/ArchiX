using System;
using System.Threading;
using System.Threading.Tasks;
using ArchiX.Library.Abstractions.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.WebApplication.Pages.Admin.Security
{
    public sealed class PasswordPolicyModel : PageModel
    {
        private readonly IPasswordPolicyAdminService _admin;

        public PasswordPolicyModel(IPasswordPolicyAdminService admin)
        {
            _admin = admin;
        }

        [BindProperty]
        public string Json { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int applicationId = 1, CancellationToken ct = default)
        {
            Json = await _admin.GetRawJsonAsync(applicationId, ct).ConfigureAwait(false);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int applicationId = 1, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(Json))
            {
                ModelState.AddModelError(string.Empty, "JSON boþ olamaz.");
                return Page();
            }

            try
            {
                await _admin.UpdateAsync(Json, applicationId, ct).ConfigureAwait(false);
                StatusMessage = "Parola politikasý güncellendi.";
                return RedirectToPage(new { applicationId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Güncelleme baþarýsýz: " + ex.Message);
                return Page();
            }
        }
    }
}
