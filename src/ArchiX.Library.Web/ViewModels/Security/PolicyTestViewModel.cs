// src/ArchiX.Library.Web/ViewModels/Security/PolicyTestViewModel.cs
namespace ArchiX.Library.Web.ViewModels.Security;

public sealed class PolicyTestViewModel
{
    public string? Password { get; set; }

    public PolicyTestResponseViewModel? Result { get; set; }
}

public sealed class PolicyTestResponseViewModel
{
    public bool IsValid { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public int StrengthScore { get; init; }

    public int? HistoryCheckResult { get; init; }

    public int? PwnedCount { get; init; }
}