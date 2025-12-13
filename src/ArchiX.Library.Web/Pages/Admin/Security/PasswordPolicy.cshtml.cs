using System.Text.Json;

using ArchiX.Library.Abstractions.Security;      // IPasswordPolicyAdminService, IPasswordPolicyProvider, PolicyNames

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Web.Pages.Admin.Security
{
    [Authorize(Policy = PolicyNames.Admin)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public sealed class PasswordPolicyModel : PageModel
    {
        private readonly IPasswordPolicyAdminService _admin;
        private readonly ILogger<PasswordPolicyModel> _logger;

        // CA1869: JsonSerializerOptions tekil instance
        private static readonly JsonSerializerOptions PrettyJson = new() { WriteIndented = true };

        public PasswordPolicyModel(IPasswordPolicyAdminService admin, ILogger<PasswordPolicyModel> logger)
        {
            _admin = admin;
            _logger = logger;
        }

        [BindProperty]
        public string Json { get; set; } = string.Empty;

        public int ApplicationId { get; private set; } = 1;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int applicationId = 1, CancellationToken ct = default)
        {
            ApplicationId = NormalizeApplicationId(applicationId);
            Json = await _admin.GetRawJsonAsync(ApplicationId, ct).ConfigureAwait(false);
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(int applicationId = 1, CancellationToken ct = default)
        {
            ApplicationId = NormalizeApplicationId(applicationId);

            if (string.IsNullOrWhiteSpace(Json))
            {
                ModelState.AddModelError(string.Empty, "JSON boþ olamaz.");
                return Page();
            }

            if (!TryValidateJson(Json, out string? normalized, out string? validationError))
            {
                ModelState.AddModelError(string.Empty, "Geçersiz JSON: " + validationError);
                return Page();
            }

            // Pretty formatlý JSON'u editörde göster
            Json = normalized ?? Json;

            try
            {
                await _admin.UpdateAsync(Json, ApplicationId, null, ct).ConfigureAwait(false);

                // Güncelleme sonrasý cache invalidate (provider varsa)
                if (HttpContext.RequestServices.GetService(typeof(IPasswordPolicyProvider)) is IPasswordPolicyProvider provider)
                    provider.Invalidate(ApplicationId);

                StatusMessage = "Parola politikasý güncellendi.";
                return RedirectToPage(new { applicationId = ApplicationId });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Parola politikasý güncellemesi iptal edildi. ApplicationId={ApplicationId}.", ApplicationId);
                ModelState.AddModelError(string.Empty, "Ýþlem iptal edildi.");
                return Page();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Parola politikasý güncellemesi reddedildi. ApplicationId={ApplicationId}.", ApplicationId);
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parola politikasý güncellemesi baþarýsýz. ApplicationId={ApplicationId}.", ApplicationId);
                ModelState.AddModelError(string.Empty, "Güncelleme baþarýsýz: " + ex.Message);
                return Page();
            }
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostFormat(int applicationId = 1)
        {
            // Editördeki JSON'u yalnýz biçimlendirir
            ApplicationId = NormalizeApplicationId(applicationId);

            if (string.IsNullOrWhiteSpace(Json))
            {
                ModelState.AddModelError(string.Empty, "JSON boþ olamaz.");
                return Page();
            }

            if (!TryValidateJson(Json, out string? normalized, out string? validationError))
            {
                ModelState.AddModelError(string.Empty, "Geçersiz JSON: " + validationError);
                return Page();
            }

            Json = normalized ?? Json;
            StatusMessage = "JSON biçimlendirildi.";
            return Page();
        }

        private static int NormalizeApplicationId(int applicationId) => applicationId > 0 ? applicationId : 1;

        // Temel alan kontrolleri + pretty format
        private static bool TryValidateJson(string raw, out string? normalized, out string? error)
        {
            normalized = null;
            error = null;

            try
            {
                using var document = JsonDocument.Parse(raw);
                var root = document.RootElement;

                // Zorunlu alanlar (camelCase)
                string[] required =
                {
                    "version","minLength","maxLength","requireUpper","requireLower","requireDigit","requireSymbol",
                    "allowedSymbols","minDistinctChars","maxRepeatedSequence","blockList","historyCount","lockoutThreshold",
                    "lockoutSeconds","hash"
                };
                foreach (var key in required)
                {
                    if (!root.TryGetProperty(key, out _))
                    {
                        error = "Eksik alan: " + key;
                        return false;
                    }
                }

                if (root.TryGetProperty("hash", out var hashElem) && hashElem.ValueKind != JsonValueKind.Object)
                {
                    error = "'hash' alaný bir nesne olmalý.";
                    return false;
                }

                if (root.TryGetProperty("minLength", out var minL) && root.TryGetProperty("maxLength", out var maxL)
                    && minL.TryGetInt32(out var minVal) && maxL.TryGetInt32(out var maxVal) && minVal > maxVal)
                {
                    error = "minLength, maxLength deðerinden büyük olamaz.";
                    return false;
                }

                // Pretty serialize (CA1869: cached options)
                normalized = JsonSerializer.Serialize(root, PrettyJson);
                return true;
            }
            catch (JsonException jex)
            {
                error = jex.Message;
                return false;
            }
            catch (Exception ex)
            {
                error = "Beklenmeyen hata: " + ex.Message;
                return false;
            }
        }
    }
}
