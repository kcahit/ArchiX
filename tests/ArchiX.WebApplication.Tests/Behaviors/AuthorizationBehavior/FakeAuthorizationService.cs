using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.WebApplication.Tests.Behaviors.AuthorizationBehavior
{
    /// <summary>
    /// Testler için yapılandırılabilir yetkilendirme servisi.
    /// </summary>
    public sealed class FakeAuthorizationService : IAuthorizationService
    {
        public bool NextResult { get; set; } = true;
        public IReadOnlyList<string>? LastPolicies { get; private set; }
        public bool LastRequireAll { get; private set; }
        public int CallCount { get; private set; }

        // Desteklenen imza 1
        public Task<bool> AuthorizeAsync(IReadOnlyList<string> policies, bool requireAll, CancellationToken ct)
        {
            CallCount++;
            LastPolicies = policies;
            LastRequireAll = requireAll;
            return Task.FromResult(NextResult);
        }

        // Desteklenen imza 2 (yansıma uyumluluğu)
        public Task<bool> AuthorizeAsync(string[] policies, bool requireAll, CancellationToken ct)
        {
            CallCount++;
            LastPolicies = policies;
            LastRequireAll = requireAll;
            return Task.FromResult(NextResult);
        }

        public Task<bool> AuthorizeAsync(string policyName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task EnsureAuthorizedAsync(string policyName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool HasClaim(string type, string? value = null)
        {
            throw new NotImplementedException();
        }

        public bool HasRole(string role)
        {
            throw new NotImplementedException();
        }
    }
}
