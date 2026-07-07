using MediatR;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Auth;

namespace PodPilot.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Handles refresh token rotation.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IIdentityService identityService;
    private readonly IJwtTokenService jwtTokenService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCommandHandler"/> class.
    /// </summary>
    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IHttpContextService httpContextService)
    {
        this.refreshTokenService = refreshTokenService;
        this.identityService = identityService;
        this.jwtTokenService = jwtTokenService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (tokenEntity, newRefreshToken) = await refreshTokenService.RotateRefreshTokenAsync(
            request.RefreshToken,
            httpContextService.IpAddress,
            cancellationToken);

        var user = await identityService.GetUserByIdAsync(tokenEntity.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var roles = await identityService.GetUserRolesAsync(user.Id, cancellationToken);
        var (accessToken, expiresIn) = jwtTokenService.GenerateAccessToken(user, roles);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = expiresIn,
            User = new UserSummary
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
            },
        };
    }
}
