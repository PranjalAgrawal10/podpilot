using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.GetOrchestratorStatus;

/// <summary>
/// Handles getting orchestrator status.
/// </summary>
public sealed class GetOrchestratorStatusQueryHandler : IRequestHandler<GetOrchestratorStatusQuery, OrchestratorStatusResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IPodOrchestrator orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrchestratorStatusQueryHandler"/> class.
    /// </summary>
    public GetOrchestratorStatusQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IPodOrchestrator orchestrator)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.orchestrator = orchestrator;
    }

    /// <inheritdoc />
    public async Task<OrchestratorStatusResponse> Handle(GetOrchestratorStatusQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var status = await orchestrator.GetStatusAsync(organizationId, cancellationToken);
        return OrchestrationMapper.ToOrchestratorStatusResponse(status);
    }
}
