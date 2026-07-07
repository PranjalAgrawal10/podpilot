using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Auth.Commands.Logout;

/// <summary>
/// Handles user logout.
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenService refreshTokenService;
    private readonly IAuditService auditService;
    private readonly ICurrentUserService currentUserService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutCommandHandler"/> class.
    /// </summary>
    public LogoutCommandHandler(
        IRefreshTokenService refreshTokenService,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IHttpContextService httpContextService)
    {
        this.refreshTokenService = refreshTokenService;
        this.auditService = auditService;
        this.currentUserService = currentUserService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeRefreshTokenAsync(
            request.RefreshToken,
            "Logged out",
            cancellationToken);

        if (currentUserService.UserId.HasValue)
        {
            await auditService.LogAsync(
                AuditAction.Logout,
                nameof(User),
                currentUserService.UserId.Value.ToString(),
                "User logged out",
                currentUserService.UserId,
                httpContextService.IpAddress,
                httpContextService.CorrelationId,
                cancellationToken);
        }

        return Unit.Value;
    }
}
