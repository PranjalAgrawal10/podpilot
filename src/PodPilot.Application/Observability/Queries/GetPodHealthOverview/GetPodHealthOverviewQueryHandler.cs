using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.GetPodHealthOverview;

/// <summary>
/// Handles getting pod health overview.
/// </summary>
public sealed class GetPodHealthOverviewQueryHandler : IRequestHandler<GetPodHealthOverviewQuery, PodHealthOverviewResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IMonitoringService monitoringService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPodHealthOverviewQueryHandler"/> class.
    /// </summary>
    public GetPodHealthOverviewQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IMonitoringService monitoringService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.monitoringService = monitoringService;
    }

    /// <inheritdoc />
    public async Task<PodHealthOverviewResponse> Handle(GetPodHealthOverviewQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var overview = await monitoringService.GetPodHealthOverviewAsync(organizationId, cancellationToken);
        return ObservabilityMapper.ToPodHealthOverviewResponse(overview);
    }
}
