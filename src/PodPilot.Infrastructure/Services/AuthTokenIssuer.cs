using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Auth;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Issues authentication tokens with organization context.
/// </summary>
public sealed class AuthTokenIssuer : IAuthTokenIssuer
{
    private readonly IIdentityService identityService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IJwtTokenService jwtTokenService;
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthTokenIssuer"/> class.
    /// </summary>
    public AuthTokenIssuer(
        IIdentityService identityService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IHttpContextService httpContextService)
    {
        this.identityService = identityService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.jwtTokenService = jwtTokenService;
        this.refreshTokenService = refreshTokenService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> IssueTokensAsync(
        Guid userId,
        Guid? organizationId,
        CancellationToken cancellationToken = default)
    {
        var user = await identityService.GetUserByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("User account is not available.");
        }

        var roles = await identityService.GetUserRolesAsync(userId, cancellationToken);
        var membership = organizationId.HasValue
            ? await organizationAuthorizationService.GetMembershipAsync(organizationId.Value, userId, cancellationToken)
            : await organizationAuthorizationService.GetDefaultMembershipAsync(userId, cancellationToken);

        if (organizationId.HasValue && membership is null)
        {
            throw new ForbiddenException("You are not a member of the selected organization.");
        }

        var orgId = membership?.OrganizationId;
        var orgRole = membership?.Role;

        var (accessToken, expiresIn) = jwtTokenService.GenerateAccessToken(user, roles, orgId, orgRole);
        var (_, refreshToken) = await refreshTokenService.GenerateRefreshTokenAsync(
            user.Id,
            httpContextService.IpAddress,
            cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn,
            User = new UserSummary
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                CurrentOrganizationId = orgId,
                CurrentOrganizationRole = orgRole.HasValue
                    ? ApplicationConstants.ToRoleName(orgRole.Value)
                    : null,
            },
        };
    }
}
