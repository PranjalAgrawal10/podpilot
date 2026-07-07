using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The access token and expiration in seconds.</returns>
    (string Token, int ExpiresIn) GenerateAccessToken(User user, IEnumerable<string> roles);

    /// <summary>
    /// Gets the user identifier from an expired access token.
    /// </summary>
    /// <param name="token">The access token.</param>
    /// <returns>The user identifier if valid; otherwise <c>null</c>.</returns>
    Guid? GetUserIdFromToken(string token);
}
