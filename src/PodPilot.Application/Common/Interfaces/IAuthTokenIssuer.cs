using PodPilot.Contracts.Auth;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Issues authentication tokens with organization context.
/// </summary>
public interface IAuthTokenIssuer
{
    /// <summary>
    /// Issues access and refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="organizationId">Optional organization context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication response with tokens.</returns>
    Task<AuthResponse> IssueTokensAsync(
        Guid userId,
        Guid? organizationId,
        CancellationToken cancellationToken = default);
}
