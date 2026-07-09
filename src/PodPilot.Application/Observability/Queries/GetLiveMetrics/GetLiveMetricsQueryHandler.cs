using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.GetLiveMetrics;

/// <summary>
/// Handles getting live metrics.
/// </summary>
public sealed class GetLiveMetricsQueryHandler : IRequestHandler<GetLiveMetricsQuery, LiveMetricsResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IMetricsAggregator metricsAggregator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLiveMetricsQueryHandler"/> class.
    /// </summary>
    public GetLiveMetricsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IMetricsAggregator metricsAggregator)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.metricsAggregator = metricsAggregator;
    }

    /// <inheritdoc />
    public async Task<LiveMetricsResponse> Handle(GetLiveMetricsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var snapshot = await metricsAggregator.GetLiveMetricsAsync(organizationId, cancellationToken);
        return ObservabilityMapper.ToLiveMetricsResponse(snapshot);
    }
}
