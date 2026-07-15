using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Persists routing decisions, scores, and outcomes.
/// </summary>
public sealed class RoutingDecisionStore : IRoutingDecisionStore
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingDecisionStore"/> class.
    /// </summary>
    public RoutingDecisionStore(IApplicationDbContext dbContext, IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task PersistDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        Guid? gatewayRequestId = null,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        var routingEvent = new RoutingEvent
        {
            OrganizationId = organizationId,
            RoutingPolicyId = decision.PolicyId,
            TaskType = decision.TaskType,
            Complexity = decision.Complexity,
            Strategy = decision.Strategy,
            SelectedProviderId = decision.Selected?.ProviderId,
            SelectedModelName = decision.Selected?.ModelName,
            OverallScore = decision.Selected?.OverallScore,
            EstimatedInputTokens = decision.EstimatedInputTokens,
            EstimatedOutputTokens = decision.EstimatedOutputTokens,
            EstimatedCostUsd = decision.EstimatedCostUsd,
            EstimatedLatencyMs = decision.EstimatedLatencyMs,
            FallbackCount = decision.FallbackCount,
            IsSimulation = decision.IsSimulation,
            GatewayRequestId = gatewayRequestId,
            DecisionReason = decision.DecisionReason,
            DecidedAt = now,
            CreatedAt = now,
        };

        await dbContext.AddRoutingEventAsync(routingEvent, cancellationToken);

        if (decision.Selected is not null && !decision.IsSimulation)
        {
            await dbContext.AddCostHistoryAsync(
                new CostHistory
                {
                    OrganizationId = organizationId,
                    AiProviderId = decision.Selected.ProviderId,
                    ModelName = decision.Selected.ModelName,
                    InputTokens = decision.EstimatedInputTokens,
                    OutputTokens = decision.EstimatedOutputTokens,
                    CostUsd = decision.EstimatedCostUsd,
                    IsPredicted = true,
                    RecordedAt = now,
                    CreatedAt = now,
                },
                cancellationToken);
        }

        foreach (var candidate in decision.ScoredCandidates.Where(c => c.ModelId != Guid.Empty).Take(25))
        {
            var existing = await dbContext.ModelScores
                .FirstOrDefaultAsync(
                    s =>
                        s.OrganizationId == organizationId &&
                        s.AiProviderModelId == candidate.ModelId &&
                        s.Strategy == decision.Strategy,
                    cancellationToken);

            if (existing is null)
            {
                await dbContext.AddModelScoreAsync(
                    new ModelScore
                    {
                        OrganizationId = organizationId,
                        AiProviderId = candidate.ProviderId,
                        AiProviderModelId = candidate.ModelId,
                        ModelName = candidate.ModelName,
                        Strategy = decision.Strategy,
                        OverallScore = candidate.OverallScore,
                        CostScore = candidate.CostScore,
                        LatencyScore = candidate.LatencyScore,
                        ReliabilityScore = candidate.ReliabilityComponentScore,
                        ContextScore = candidate.ContextScore,
                        FeaturesScore = candidate.FeaturesScore,
                        AvailabilityScore = candidate.AvailabilityScore,
                        ScoredAt = now,
                        CreatedAt = now,
                    },
                    cancellationToken);
            }
            else
            {
                existing.OverallScore = candidate.OverallScore;
                existing.CostScore = candidate.CostScore;
                existing.LatencyScore = candidate.LatencyScore;
                existing.ReliabilityScore = candidate.ReliabilityComponentScore;
                existing.ContextScore = candidate.ContextScore;
                existing.FeaturesScore = candidate.FeaturesScore;
                existing.AvailabilityScore = candidate.AvailabilityScore;
                existing.ScoredAt = now;
                existing.UpdatedAt = now;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RecordOutcomeAsync(
        Guid organizationId,
        Guid providerId,
        string? modelName,
        int latencyMs,
        int inputTokens,
        int outputTokens,
        decimal? actualCostUsd = null,
        bool wasColdStart = false,
        CancellationToken cancellationToken = default)
    {
        var now = dateTimeService.UtcNow;
        await dbContext.AddLatencyHistoryAsync(
            new LatencyHistory
            {
                OrganizationId = organizationId,
                AiProviderId = providerId,
                ModelName = modelName,
                LatencyMs = latencyMs,
                QueueDepth = 0,
                PodLoadPercent = 0,
                WasColdStart = wasColdStart,
                ColdStartMs = wasColdStart ? latencyMs : null,
                RecordedAt = now,
                CreatedAt = now,
            },
            cancellationToken);

        if (actualCostUsd.HasValue)
        {
            await dbContext.AddCostHistoryAsync(
                new CostHistory
                {
                    OrganizationId = organizationId,
                    AiProviderId = providerId,
                    ModelName = modelName,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens,
                    CostUsd = actualCostUsd.Value,
                    IsPredicted = false,
                    RecordedAt = now,
                    CreatedAt = now,
                },
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
