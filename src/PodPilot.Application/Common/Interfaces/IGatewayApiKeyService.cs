using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Manages gateway API keys.
/// </summary>
public interface IGatewayApiKeyService
{
    /// <summary>
    /// Validates an API key and returns the auth context.
    /// </summary>
    Task<GatewayAuthContext?> ValidateKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new API key and returns the plaintext key once.
    /// </summary>
    Task<(GatewayApiKey Entity, string PlaintextKey)> CreateKeyAsync(
        Guid organizationId,
        Guid? userId,
        string name,
        DateTime? expiresAt,
        int rateLimitPerMinute,
        int rateLimitPerDay,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an API key.
    /// </summary>
    Task RevokeKeyAsync(
        Guid keyId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates an API key and returns the new plaintext key once.
    /// </summary>
    Task<(GatewayApiKey Entity, string PlaintextKey)> RotateKeyAsync(
        Guid keyId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
