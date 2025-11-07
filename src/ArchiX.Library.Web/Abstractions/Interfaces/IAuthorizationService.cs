namespace ArchiX.Library.Web.Abstractions.Interfaces
{
 public interface IAuthorizationService
 {
 Task<bool> AuthorizeAsync(IReadOnlyList<string> policies, bool requireAll, CancellationToken ct);
 Task<bool> AuthorizeAsync(string[] policies, bool requireAll, CancellationToken ct);
 Task<bool> AuthorizeAsync(string policyName, CancellationToken cancellationToken = default);
 Task EnsureAuthorizedAsync(string policyName, CancellationToken cancellationToken = default);
 bool HasClaim(string type, string? value = null);
 bool HasRole(string role);
 }
}
