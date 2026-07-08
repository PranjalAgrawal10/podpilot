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
    private readonly IAuthTokenIssuer authTokenIssuer;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCommandHandler"/> class.
    /// </summary>
    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IAuthTokenIssuer authTokenIssuer,
        IHttpContextService httpContextService)
    {
        this.refreshTokenService = refreshTokenService;
        this.authTokenIssuer = authTokenIssuer;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (tokenEntity, _) = await refreshTokenService.RotateRefreshTokenAsync(
            request.RefreshToken,
            httpContextService.IpAddress,
            cancellationToken);

        return await authTokenIssuer.IssueTokensAsync(
            tokenEntity.UserId,
            organizationId: null,
            cancellationToken);
    }
}
