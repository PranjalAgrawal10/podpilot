using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Routing.Queries.GetRoutingDashboard;

/// <summary>Gets the intelligent routing dashboard.</summary>
public sealed class GetRoutingDashboardQuery : IRequest<RoutingDashboardResponse>
{
}

/// <summary>Handles <see cref="GetRoutingDashboardQuery"/>.</summary>
public sealed class GetRoutingDashboardQueryHandler : IRequestHandler<GetRoutingDashboardQuery, RoutingDashboardResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IRoutingPolicy routingPolicy;

    /// <summary>Initializes a new instance of the <see cref="GetRoutingDashboardQueryHandler"/> class.</summary>
    public GetRoutingDashboardQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IRoutingPolicy routingPolicy)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.routingPolicy = routingPolicy;
    }

    /// <inheritdoc />
    public async Task<RoutingDashboardResponse> Handle(
        GetRoutingDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = RoutingAccess.RequireOrganizationContext(currentUserService);
        await RoutingAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.RoutingRead, cancellationToken);

        var policy = await routingPolicy.GetActivePolicyAsync(organizationId, null, cancellationToken);
        var since = DateTime.UtcNow.AddDays(-30);

        var latest = await dbContext.RoutingEvents
            .AsNoTracking()
            .Include(e => e.SelectedProvider)
            .Where(e => e.OrganizationId == organizationId && !e.IsSimulation)
            .OrderByDescending(e => e.DecidedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var fallbackCount = await dbContext.RoutingEvents
            .AsNoTracking()
            .Where(e => e.OrganizationId == organizationId && e.DecidedAt >= since && e.FallbackCount > 0)
            .SumAsync(e => (int?)e.FallbackCount, cancellationToken) ?? 0;

        var mostUsed = await dbContext.RoutingEvents
            .AsNoTracking()
            .Where(e =>
                e.OrganizationId == organizationId &&
                e.DecidedAt >= since &&
                !e.IsSimulation &&
                e.SelectedModelName != null)
            .GroupBy(e => e.SelectedModelName!)
            .Select(g => new RoutingModelUsageItem { ModelName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        var providerRanking = await dbContext.ModelScores
            .AsNoTracking()
            .Include(s => s.AiProvider)
            .Where(s => s.OrganizationId == organizationId)
            .GroupBy(s => new { s.AiProviderId, s.AiProvider.DisplayName })
            .Select(g => new RoutingProviderRankItem
            {
                ProviderId = g.Key.AiProviderId,
                ProviderName = g.Key.DisplayName,
                Score = g.Average(x => x.OverallScore),
                LatencyMs = (int)g.Average(x => x.LatencyScore),
                AvailabilityScore = g.Average(x => x.AvailabilityScore),
            })
            .OrderByDescending(x => x.Score)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new RoutingDashboardResponse
        {
            CurrentModel = latest?.SelectedModelName,
            CurrentProvider = latest?.SelectedProvider?.DisplayName,
            CurrentProviderId = latest?.SelectedProviderId,
            Strategy = policy?.Strategy.ToString() ?? Domain.Enums.RoutingStrategy.Balanced.ToString(),
            EstimatedCostUsd = latest?.EstimatedCostUsd ?? 0,
            EstimatedLatencyMs = latest?.EstimatedLatencyMs ?? 0,
            FallbackCount = fallbackCount,
            MostUsedModels = mostUsed,
            ProviderRanking = providerRanking,
        };
    }
}
