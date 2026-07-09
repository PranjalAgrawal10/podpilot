using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Commands.ScaleUp;

/// <summary>
/// Handles manual scale-up.
/// </summary>
public sealed class ScaleUpCommandHandler : IRequestHandler<ScaleUpCommand, ScalingActionResult>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IAutoScaler autoScaler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScaleUpCommandHandler"/> class.
    /// </summary>
    public ScaleUpCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IAutoScaler autoScaler)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.autoScaler = autoScaler;
    }

    /// <inheritdoc />
    public async Task<ScalingActionResult> Handle(ScaleUpCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorManage,
            cancellationToken);

        return await autoScaler.ScaleUpAsync(
            organizationId,
            request.PoolId,
            request.Reason ?? "Manual scale-up requested.",
            cancellationToken);
    }
}
