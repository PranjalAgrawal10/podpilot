using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user and organization context.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The user's ASP.NET Identity roles.</param>
    /// <param name="organizationId">The current organization identifier.</param>
    /// <param name="organizationRole">The user's role in the current organization.</param>
    /// <returns>The access token and expiration in seconds.</returns>
    (string Token, int ExpiresIn) GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        Guid? organizationId = null,
        OrganizationRole? organizationRole = null);

    /// <summary>
    /// Gets the user identifier from an expired access token.
    /// </summary>
    /// <param name="token">The access token.</param>
    /// <returns>The user identifier if valid; otherwise <c>null</c>.</returns>
    Guid? GetUserIdFromToken(string token);
}
