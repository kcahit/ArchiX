// File: ArchiX.WebApplication/Abstractions/ICurrentUser.cs
namespace ArchiX.WebApplication.Abstractions
{
    /// <summary>
    /// Provides information about the currently authenticated user.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// Gets the user identifier if available.
        /// </summary>
        string? UserId { get; }

        /// <summary>
        /// Gets the display name or username if available.
        /// </summary>
        string? UserName { get; }

        /// <summary>
        /// Indicates whether the current user is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the roles associated with the current user.
        /// </summary>
        System.Collections.Generic.IReadOnlyCollection<string> Roles { get; }

        /// <summary>
        /// Returns true if the user is in the specified role.
        /// </summary>
        /// <param name="role">Role name to check.</param>
        bool IsInRole(string role);
    }
}
