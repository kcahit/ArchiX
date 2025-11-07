using ArchiX.WebApplication.Abstractions.Interfaces;
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

 public Task<bool> AuthorizeAsync(string policyName, CancellationToken cancellationToken = default)
 {
 throw new System.NotImplementedException();
 }

 public Task EnsureAuthorizedAsync(string policyName, CancellationToken cancellationToken = default)
 {
 throw new System.NotImplementedException();
 }

 public bool HasClaim(string type, string? value = null)
 {
 throw new System.NotImplementedException();
 }

 public bool HasRole(string role)
 {
 throw new System.NotImplementedException();
 }
 }
}
