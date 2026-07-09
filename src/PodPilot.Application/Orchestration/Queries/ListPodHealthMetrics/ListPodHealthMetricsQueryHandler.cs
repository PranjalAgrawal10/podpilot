using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Orchestration.Queries.ListPodHealthMetrics;

/// <summary>
/// Handles listing pod health metrics.
/// </summary>
public sealed class ListPodHealthMetricsQueryHandler : IRequestHandler<ListPodHealthMetricsQuery, IReadOnlyList<PodHealthMetricResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListPodHealthMetricsQueryHandler"/> class.
    /// </summary>
    public ListPodHealthMetricsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PodHealthMetricResponse>> Handle(ListPodHealthMetricsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorRead,
            cancellationToken);

        var query = dbContext.PodHealthMetrics
            .Where(m => m.OrganizationId == organizationId);

        if (request.PodId.HasValue)
        {
            query = query.Where(m => m.GpuPodId == request.PodId.Value);
        }

        var metrics = await query
            .OrderByDescending(m => m.RecordedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return metrics.Select(OrchestrationMapper.ToHealthMetricResponse).ToList();
    }
}
