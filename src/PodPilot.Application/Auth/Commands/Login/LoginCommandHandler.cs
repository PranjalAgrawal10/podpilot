using MediatR;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Auth;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Auth.Commands.Login;

/// <summary>
/// Handles user login.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService identityService;
    private readonly IJwtTokenService jwtTokenService;
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// </summary>
    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.identityService = identityService;
        this.jwtTokenService = jwtTokenService;
        this.refreshTokenService = refreshTokenService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var roles = await identityService.GetUserRolesAsync(user.Id, cancellationToken);
        var (accessToken, expiresIn) = jwtTokenService.GenerateAccessToken(user, roles);
        var (_, refreshToken) = await refreshTokenService.GenerateRefreshTokenAsync(
            user.Id,
            httpContextService.IpAddress,
            cancellationToken);

        await auditService.LogAsync(
            AuditAction.Login,
            nameof(User),
            user.Id.ToString(),
            "User logged in",
            user.Id,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
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
            },
        };
    }
}
