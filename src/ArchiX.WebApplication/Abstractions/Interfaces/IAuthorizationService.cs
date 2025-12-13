// File: ArchiX.WebApplication/Abstractions/IAuthorizationService.cs
namespace ArchiX.WebApplication.Abstractions.Interfaces
{
    /// <summary>
    /// Evaluates authorization for the current user against roles, claims, and policies.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Returns true if the current user satisfies the given policy.
        /// </summary>
        /// <param name="policyName">Policy identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<bool> AuthorizeAsync(string policyName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Throws if the current user does not satisfy the policy.
        /// </summary>
        /// <param name="policyName">Policy identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task EnsureAuthorizedAsync(string policyName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns true if the current user has the specified role.
        /// </summary>
        /// <param name="role">Role name.</param>
        bool HasRole(string role);

        /// <summary>
        /// Returns true if the current user has the specified claim.
        /// </summary>
        /// <param name="type">Claim type.</param>
        /// <param name="value">Optional claim value to match.</param>
        bool HasClaim(string type, string? value = null);
    }
}
