using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Text;

using ArchiX.Library.Abstractions.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.WebHost.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[AutoValidateAntiforgeryToken]
public sealed class BlacklistModel : PageModel
{
    private const int MaxBulkWordCount = 100;
    private static readonly SearchValues<char> CsvSpecialChars = SearchValues.Create(['"', ',', '\n', '\r']);

    private readonly IPasswordPolicyAdminService _adminService;

    public BlacklistModel(IPasswordPolicyAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    [BindProperty]
    [Display(Name = "Kelime")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Kelime 2-256 karakter aralığında olmalıdır.")]
    public string? NewWord { get; set; }

    [BindProperty]
    [Display(Name = "Toplu kelimeler (her satıra bir kelime)")]
    public string? BulkWords { get; set; }

    public IReadOnlyList<PasswordBlacklistWordDto> Words { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        await LoadAsync(ct).ConfigureAwait(false);
        return Page();
    }

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
            ModelState.AddModelError(nameof(NewWord), "Kelime boş olamaz.");
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

    public async Task<IActionResult> OnPostBulkAddAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);

        if (string.IsNullOrWhiteSpace(BulkWords))
        {
            ModelState.AddModelError(nameof(BulkWords), "Kelime listesi boş olamaz.");
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }

        var parsedWords = BulkWords
            .Split(['\r', '\n', ';', ',', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim())
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (parsedWords.Length == 0)
        {
            ModelState.AddModelError(nameof(BulkWords), "En az bir kelime girin.");
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }

        if (parsedWords.Length > MaxBulkWordCount)
        {
            ModelState.AddModelError(nameof(BulkWords), $"En fazla {MaxBulkWordCount} kelime ekleyebilirsiniz.");
            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }

        var userId = GetUserId();
        var addedCount = 0;
        var duplicateCount = 0;
        var invalidEntries = new List<string>();

        foreach (var word in parsedWords)
        {
            if (word.Length < 2 || word.Length > 256)
            {
                invalidEntries.Add(word);
                continue;
            }

            var added = await _adminService.TryAddBlacklistWordAsync(ApplicationId, word, userId, ct).ConfigureAwait(false);
            if (added)
            {
                addedCount++;
            }
            else
            {
                duplicateCount++;
            }
        }

        if (addedCount == 0)
        {
            if (duplicateCount > 0)
            {
                ModelState.AddModelError(nameof(BulkWords), "Girilen kelimeler zaten listede.");
            }

            if (invalidEntries.Count > 0)
            {
                var sample = string.Join(", ", invalidEntries.Take(5));
                var suffix = invalidEntries.Count > 5 ? "..." : string.Empty;
                ModelState.AddModelError(nameof(BulkWords), $"Geçersiz kelimeler: {sample}{suffix}");
            }

            await LoadAsync(ct).ConfigureAwait(false);
            return Page();
        }

        var message = new StringBuilder();
        message.Append($"{addedCount} kelime eklendi.");
        if (duplicateCount > 0)
        {
            message.Append($" {duplicateCount} kelime zaten listede.");
        }

        if (invalidEntries.Count > 0)
        {
            message.Append($" {invalidEntries.Count} kelime geçersiz olduğu için atlandı.");
        }

        StatusMessage = message.ToString();
        return RedirectToPage(new { applicationId = ApplicationId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        if (id <= 0)
        {
            StatusMessage = "Geçersiz kayıt.";
            return RedirectToPage(new { applicationId = ApplicationId });
        }

        var removed = await _adminService.TryRemoveBlacklistWordAsync(id, GetUserId(), ct).ConfigureAwait(false);
        StatusMessage = removed ? "Kelime silindi." : "Kayıt bulunamadı.";
        return RedirectToPage(new { applicationId = ApplicationId });
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeAppId(ApplicationId);
        var entries = await _adminService.GetBlacklistAsync(ApplicationId, ct).ConfigureAwait(false);

        var builder = new StringBuilder();
        builder.AppendLine("Word,CreatedBy,CreatedAtUtc,IsActive");

        foreach (var entry in entries)
        {
            builder.AppendLine(string.Join(',',
                CsvEscape(entry.Word),
                CsvEscape(entry.CreatedBy),
                entry.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                entry.IsActive ? "true" : "false"));
        }

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        var fileName = $"password-blacklist-app{ApplicationId}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(bytes, "text/csv", fileName);
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

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.AsSpan().ContainsAny(CsvSpecialChars))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
