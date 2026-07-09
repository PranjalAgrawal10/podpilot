using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.GetAutoScalerStatus;

/// <summary>
/// Handles getting auto-scaler status.
/// </summary>
public sealed class GetAutoScalerStatusQueryHandler : IRequestHandler<GetAutoScalerStatusQuery, AutoScalerStatusResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IAutoScaler autoScaler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutoScalerStatusQueryHandler"/> class.
    /// </summary>
    public GetAutoScalerStatusQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IAutoScaler autoScaler)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.autoScaler = autoScaler;
    }

    /// <inheritdoc />
    public async Task<AutoScalerStatusResponse> Handle(GetAutoScalerStatusQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var status = await autoScaler.GetStatusAsync(organizationId, cancellationToken);
        return OrchestrationMapper.ToAutoScalerStatusResponse(status);
    }
}
