namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Provides information about the current authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's identifier.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's email.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's roles.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
