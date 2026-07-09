using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.GetProviderHealthOverview;

/// <summary>
/// Handles getting provider health overview.
/// </summary>
public sealed class GetProviderHealthOverviewQueryHandler : IRequestHandler<GetProviderHealthOverviewQuery, ProviderHealthOverviewResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IMonitoringService monitoringService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProviderHealthOverviewQueryHandler"/> class.
    /// </summary>
    public GetProviderHealthOverviewQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IMonitoringService monitoringService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.monitoringService = monitoringService;
    }

    /// <inheritdoc />
    public async Task<ProviderHealthOverviewResponse> Handle(GetProviderHealthOverviewQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var overview = await monitoringService.GetProviderHealthOverviewAsync(organizationId, cancellationToken);
        return ObservabilityMapper.ToProviderHealthOverviewResponse(overview);
    }
}
