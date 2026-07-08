using MediatR;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Auth;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Organizations.Commands.SwitchOrganization;

/// <summary>
/// Handles switching the current organization context.
/// </summary>
public sealed class SwitchOrganizationCommandHandler : IRequestHandler<SwitchOrganizationCommand, AuthResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IAuthTokenIssuer authTokenIssuer;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchOrganizationCommandHandler"/> class.
    /// </summary>
    public SwitchOrganizationCommandHandler(
        ICurrentUserService currentUserService,
        IAuthTokenIssuer authTokenIssuer,
        IAuditService auditService,
        IHttpContextService httpContextService)
    {
        this.currentUserService = currentUserService;
        this.authTokenIssuer = authTokenIssuer;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
    }

    /// <inheritdoc />
    public async Task<AuthResponse> Handle(SwitchOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var userId = currentUserService.UserId.Value;
        var response = await authTokenIssuer.IssueTokensAsync(userId, request.OrganizationId, cancellationToken);

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(Organization),
            request.OrganizationId.ToString(),
            "Switched current organization context",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        return response;
    }
}
