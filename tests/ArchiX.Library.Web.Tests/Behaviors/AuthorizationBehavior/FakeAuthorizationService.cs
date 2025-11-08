using ArchiX.Library.Web.Abstractions.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiX.Library.Web.Tests.Behaviors.AuthorizationBehavior
{
 public sealed class FakeAuthorizationService : IAuthorizationService
 {
 public bool NextResult { get; set; } = true;
 public System.Collections.Generic.IReadOnlyList<string>? LastPolicies { get; private set; }
 public bool LastRequireAll { get; private set; }
 public int CallCount { get; private set; }

 public Task<bool> AuthorizeAsync(System.Collections.Generic.IReadOnlyList<string> policies, bool requireAll, CancellationToken ct)
 {
 CallCount++;
 LastPolicies = policies;
 LastRequireAll = requireAll;
 return Task.FromResult(NextResult);
 }

 public Task<bool> AuthorizeAsync(string[] policies, bool requireAll, CancellationToken ct)
 {
 CallCount++;
 LastPolicies = policies;
 LastRequireAll = requireAll;
 return Task.FromResult(NextResult);
 }

 public Task<bool> AuthorizeAsync(string policyName, CancellationToken cancellationToken = default) => Task.FromResult(NextResult);
 public Task EnsureAuthorizedAsync(string policyName, CancellationToken cancellationToken = default) => Task.CompletedTask;
 public bool HasClaim(string type, string? value = null) => false;
 public bool HasRole(string role) => false;
 }
}
