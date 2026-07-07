using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Service for managing refresh tokens with rotation.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new refresh token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="ipAddress">The client IP address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated refresh token entity and raw token value.</returns>
    Task<(RefreshToken Entity, string Token)> GenerateRefreshTokenAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a refresh token, revoking the old one and issuing a new one.
    /// </summary>
    /// <param name="token">The current refresh token.</param>
    /// <param name="ipAddress">The client IP address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new refresh token entity and raw token value.</returns>
    Task<(RefreshToken Entity, string Token)> RotateRefreshTokenAsync(
        string token,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="token">The refresh token to revoke.</param>
    /// <param name="reason">The revocation reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RevokeRefreshTokenAsync(
        string token,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="reason">The revocation reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RevokeAllUserTokensAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default);
}
