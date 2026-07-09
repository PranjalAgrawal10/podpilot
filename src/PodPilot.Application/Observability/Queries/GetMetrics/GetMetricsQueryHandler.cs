using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Observability.Queries.GetMetrics;

/// <summary>
/// Handles getting historical metrics snapshots.
/// </summary>
public sealed class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, IReadOnlyList<MetricsSnapshotResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMetricsQueryHandler"/> class.
    /// </summary>
    public GetMetricsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MetricsSnapshotResponse>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = ObservabilityAccess.RequireOrganizationContext(currentUserService);

        await ObservabilityAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.ObservabilityRead,
            cancellationToken);

        var query = dbContext.MetricsSnapshots
            .Where(m => m.OrganizationId == organizationId);

        if (request.From.HasValue)
        {
            query = query.Where(m => m.RecordedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(m => m.RecordedAt <= request.To.Value);
        }

        if (request.ProviderId.HasValue)
        {
            query = query.Where(m => m.ProviderId == request.ProviderId.Value);
        }

        if (request.PodId.HasValue)
        {
            query = query.Where(m => m.GpuPodId == request.PodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ModelName))
        {
            query = query.Where(m => m.ModelName == request.ModelName);
        }

        var snapshots = await query
            .OrderByDescending(m => m.RecordedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return snapshots.Select(ObservabilityMapper.ToMetricsSnapshotResponse).ToList();
    }
}
