using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Scores provider availability from health and recent failure signals.
/// </summary>
public sealed class AvailabilityScorer : IAvailabilityScorer
{
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailabilityScorer"/> class.
    /// </summary>
    public AvailabilityScorer(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<double> ScoreAsync(
        Guid organizationId,
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var provider = await dbContext.AiInferenceProviders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == providerId && p.OrganizationId == organizationId,
                cancellationToken);

        if (provider is null || !provider.IsEnabled)
        {
            return 0;
        }

        if (!provider.IsValidated)
        {
            return 25;
        }

        var health = await dbContext.AiProviderHealthSnapshots
            .AsNoTracking()
            .Where(h => h.AiProviderId == providerId)
            .OrderByDescending(h => h.LastCheckedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (health is null)
        {
            return 70;
        }

        var baseScore = health.Status switch
        {
            AiProviderHealthState.Healthy => 95,
            AiProviderHealthState.Degraded => 60,
            AiProviderHealthState.Unhealthy => 20,
            AiProviderHealthState.Unknown => 50,
            _ => 50,
        };

        var errorPenalty = Math.Clamp(health.ErrorRate * 100, 0, 40);
        var failurePenalty = Math.Min(30, health.ConsecutiveFailures * 5);
        return Math.Clamp(baseScore - errorPenalty - failurePenalty, 0, 100);
    }
}
