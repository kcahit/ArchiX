// src/ArchiX.Library.Web/ViewModels/Security/AuditTrailViewModel.cs
namespace ArchiX.Library.Web.ViewModels.Security;

public sealed class AuditTrailViewModel
{
    public int ApplicationId { get; init; }

    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public string? UserFilter { get; init; }

    public IReadOnlyList<AuditEntryViewModel> Entries { get; init; } = Array.Empty<AuditEntryViewModel>();
}

public sealed class AuditEntryViewModel
{
    public int AuditId { get; init; }

    public DateTimeOffset ChangedAt { get; init; }

    public string UserDisplayName { get; init; } = string.Empty;

    public string ActionSummary { get; init; } = string.Empty;

    public string OldJson { get; init; } = string.Empty;

    public string NewJson { get; init; } = string.Empty;
}

public sealed class AuditDiffViewModel
{
    public int AuditId { get; init; }

    public string HtmlDiff { get; init; } = string.Empty;
}