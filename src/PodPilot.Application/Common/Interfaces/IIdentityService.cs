using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Service for user identity operations backed by ASP.NET Identity.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="password">The password.</param>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user and any errors.</returns>
    Task<(User? User, string[] Errors)> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates user credentials.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authenticated user if valid; otherwise <c>null</c>.</returns>
    Task<User?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise <c>null</c>.</returns>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise <c>null</c>.</returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the roles assigned to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's roles.</returns>
    Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AssignRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures default roles exist in the identity store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task EnsureRolesExistAsync(CancellationToken cancellationToken = default);
}
