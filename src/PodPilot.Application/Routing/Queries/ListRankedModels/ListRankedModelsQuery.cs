using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Routing.Queries.ListRankedModels;

/// <summary>Lists ranked models for intelligent routing.</summary>
public sealed class ListRankedModelsQuery : IRequest<IReadOnlyList<RankedModelResponse>>
{
}

/// <summary>Handles <see cref="ListRankedModelsQuery"/>.</summary>
public sealed class ListRankedModelsQueryHandler : IRequestHandler<ListRankedModelsQuery, IReadOnlyList<RankedModelResponse>>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>Initializes a new instance of the <see cref="ListRankedModelsQueryHandler"/> class.</summary>
    public ListRankedModelsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RankedModelResponse>> Handle(
        ListRankedModelsQuery request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = RoutingAccess.RequireOrganizationContext(currentUserService);
        await RoutingAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.RoutingRead, cancellationToken);

        var scores = await dbContext.ModelScores
            .AsNoTracking()
            .Include(s => s.AiProvider)
            .Where(s => s.OrganizationId == organizationId)
            .OrderByDescending(s => s.OverallScore)
            .ThenBy(s => s.ModelName)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (scores.Count > 0)
        {
            return scores.Select(RoutingMapper.ToRankedModelResponse).ToList();
        }

        // Fall back to live catalog when scores have not been persisted yet.
        var catalog = await dbContext.AiProviderModels
            .AsNoTracking()
            .Include(m => m.AiProvider)
            .Where(m =>
                m.OrganizationId == organizationId &&
                m.IsEnabled &&
                m.AiProvider.IsEnabled &&
                m.AiProvider.IsValidated)
            .OrderBy(m => m.AiProvider.Priority)
            .ThenBy(m => m.ModelName)
            .Take(100)
            .ToListAsync(cancellationToken);

        return catalog.Select(m => new RankedModelResponse
        {
            ProviderId = m.AiProviderId,
            ProviderName = m.AiProvider.DisplayName,
            ModelId = m.Id,
            ModelName = m.ModelName,
            Strategy = Domain.Enums.RoutingStrategy.Balanced.ToString(),
            OverallScore = (m.QualityScore + m.SpeedScore + m.ReliabilityScore) / 3.0,
            CostScore = 50,
            LatencyScore = m.SpeedScore,
            ReliabilityScore = m.ReliabilityScore,
            ContextScore = m.ContextLength.HasValue ? Math.Min(100, m.ContextLength.Value / 1280.0) : 40,
            FeaturesScore = m.QualityScore,
            AvailabilityScore = 70,
            ScoredAt = m.SyncedAt,
        }).OrderByDescending(x => x.OverallScore).ToList();
    }
}
