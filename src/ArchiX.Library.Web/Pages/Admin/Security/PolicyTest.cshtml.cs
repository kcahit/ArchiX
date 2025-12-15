using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Runtime.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArchiX.Library.Web.Pages.Admin.Security;

[Authorize(Policy = PolicyNames.Admin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PolicyTestModel : PageModel
{
    private readonly PasswordValidationService _validationService;
    private readonly IPasswordPolicyProvider _policyProvider;

    public PolicyTestModel(
        PasswordValidationService validationService,
        IPasswordPolicyProvider policyProvider)
    {
        _validationService = validationService;
        _policyProvider = policyProvider;
    }

    [BindProperty(SupportsGet = true)]
    public int ApplicationId { get; set; } = 1;

    [BindProperty]
    public PolicyTestForm Form { get; set; } = new();

    public PasswordPolicyOptions? ActivePolicy { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeApplicationId(ApplicationId);
        ActivePolicy = await _policyProvider.GetAsync(ApplicationId, ct).ConfigureAwait(false);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        ApplicationId = NormalizeApplicationId(ApplicationId);
        ActivePolicy = await _policyProvider.GetAsync(ApplicationId, ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(Form.Password))
        {
            ModelState.AddModelError(nameof(Form.Password), "Parola boþ olamaz.");
            return Page();
        }

        var result = await _validationService.ValidateAsync(Form.Password, 0, ApplicationId, ct).ConfigureAwait(false);

        Form.Result = new PolicyTestResult
        {
            IsValid = result.IsValid,
            Errors = result.Errors,
            StrengthScore = CalculateStrengthScore(Form.Password),
            HistoryCheckResult = result.Errors.Contains("HISTORY") ? 1 : null,
            PwnedCount = result.Errors.Contains("PWNED") ? 1 : 0
        };

        return Page();
    }

    private static int NormalizeApplicationId(int value) => value > 0 ? value : 1;

    private static int CalculateStrengthScore(string password)
    {
        if (string.IsNullOrEmpty(password))
            return 0;

        var score = Math.Min(40, password.Length * 2);

        if (password.Any(char.IsLower)) score += 15;
        if (password.Any(char.IsUpper)) score += 15;
        if (password.Any(char.IsDigit)) score += 15;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 15;

        return Math.Clamp(score, 0, 100);
    }
}

public sealed class PolicyTestForm
{
    public string Password { get; set; } = string.Empty;
    public PolicyTestResult? Result { get; set; }
}

public sealed class PolicyTestResult
{
    public bool IsValid { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = [];
    public int StrengthScore { get; set; }
    public int? HistoryCheckResult { get; set; }
    public int? PwnedCount { get; set; }
}
