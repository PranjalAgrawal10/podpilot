using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Commands.ScaleDown;

/// <summary>
/// Handles manual scale-down.
/// </summary>
public sealed class ScaleDownCommandHandler : IRequestHandler<ScaleDownCommand, ScalingActionResult>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IAutoScaler autoScaler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScaleDownCommandHandler"/> class.
    /// </summary>
    public ScaleDownCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IAutoScaler autoScaler)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.autoScaler = autoScaler;
    }

    /// <inheritdoc />
    public async Task<ScalingActionResult> Handle(ScaleDownCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorManage,
            cancellationToken);

        return await autoScaler.ScaleDownAsync(
            organizationId,
            request.PoolId,
            request.Reason ?? "Manual scale-down requested.",
            cancellationToken);
    }
}
