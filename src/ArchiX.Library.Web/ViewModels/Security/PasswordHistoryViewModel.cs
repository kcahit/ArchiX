// src/ArchiX.Library.Web/ViewModels/Security/PasswordHistoryViewModel.cs
namespace ArchiX.Library.Web.ViewModels.Security;

public sealed class PasswordHistoryViewModel
{
    public string? Query { get; init; }

    public IReadOnlyList<UserPasswordHistoryViewModel> Entries { get; init; } = Array.Empty<UserPasswordHistoryViewModel>();
}

public sealed class UserPasswordHistoryViewModel
{
    public int UserId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string MaskedHash { get; init; } = string.Empty;

    public string HashAlgorithm { get; init; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; init; }

    public bool IsExpired { get; init; }
}