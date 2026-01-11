// src/ArchiX.Library.Web/ViewModels/Security/SecurityDashboardViewModel.cs
using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Web.ViewModels.Security;

public sealed class SecurityDashboardViewModel
{
    public required PasswordPolicyOptions ActivePolicy { get; init; }

    public int BlacklistWordCount { get; init; }

    public int ExpiredPasswordCount { get; init; }

    public Dictionary<string, int> Last30DaysErrors { get; init; } = new();

    public IReadOnlyList<RecentAuditEntry> RecentChanges { get; init; } = Array.Empty<RecentAuditEntry>();
}

public sealed class RecentAuditEntry
{
    public int AuditId { get; init; }

    public string UserDisplayName { get; init; } = string.Empty;

    public DateTimeOffset ChangedAt { get; init; }

    public string Summary { get; init; } = string.Empty;
}