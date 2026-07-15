using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Orchestrates intelligent routing: analyze → score → select → persist.
/// </summary>
public sealed class RoutingEngine : IRoutingEngine
{
    private readonly ITaskClassifier taskClassifier;
    private readonly IProviderSelector providerSelector;
    private readonly IModelRouter modelRouter;
    private readonly IRoutingPolicy routingPolicy;
    private readonly ICostEstimator costEstimator;
    private readonly ILatencyPredictor latencyPredictor;
    private readonly IAvailabilityScorer availabilityScorer;
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;
    private readonly IRoutingNotificationService notificationService;
    private readonly ILogger<RoutingEngine> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingEngine"/> class.
    /// </summary>
    public RoutingEngine(
        ITaskClassifier taskClassifier,
        IProviderSelector providerSelector,
        IModelRouter modelRouter,
        IRoutingPolicy routingPolicy,
        ICostEstimator costEstimator,
        ILatencyPredictor latencyPredictor,
        IAvailabilityScorer availabilityScorer,
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        IRoutingNotificationService notificationService,
        ILogger<RoutingEngine> logger)
    {
        this.taskClassifier = taskClassifier;
        this.providerSelector = providerSelector;
        this.modelRouter = modelRouter;
        this.routingPolicy = routingPolicy;
        this.costEstimator = costEstimator;
        this.latencyPredictor = latencyPredictor;
        this.availabilityScorer = availabilityScorer;
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public Task<RoutingDecision> SimulateAsync(
        RoutingEngineRequest request,
        CancellationToken cancellationToken = default) =>
        RouteAsync(
            new RoutingEngineRequest
            {
                OrganizationId = request.OrganizationId,
                Path = request.Path,
                BodyJson = request.BodyJson,
                Prompt = request.Prompt,
                StrategyOverride = request.StrategyOverride,
                ModelHint = request.ModelHint,
                IsSimulation = true,
                GatewayRequestId = request.GatewayRequestId,
            },
            cancellationToken);

    /// <inheritdoc />
    public async Task<RoutingDecision> RouteAsync(
        RoutingEngineRequest request,
        CancellationToken cancellationToken = default)
    {
        var analysis = taskClassifier.Analyze(request.Path, request.BodyJson, request.Prompt);
        if (!string.IsNullOrWhiteSpace(request.ModelHint))
        {
            analysis = new RoutingRequestAnalysis
            {
                TaskType = analysis.TaskType,
                Complexity = analysis.Complexity,
                EstimatedInputTokens = analysis.EstimatedInputTokens,
                EstimatedOutputTokens = analysis.EstimatedOutputTokens,
                RequestedModel = request.ModelHint,
                RequiresVision = analysis.RequiresVision,
                RequiresEmbeddings = analysis.RequiresEmbeddings,
                RequiresTools = analysis.RequiresTools,
                RequiresReasoning = analysis.RequiresReasoning,
                PromptPreview = analysis.PromptPreview,
            };
        }

        var policy = await routingPolicy.GetActivePolicyAsync(
            request.OrganizationId,
            analysis.RequestedModel,
            cancellationToken);

        var strategy = request.StrategyOverride
                       ?? policy?.Strategy
                       ?? RoutingStrategy.Balanced;

        if (strategy == RoutingStrategy.ProviderPriority &&
            policy?.PrimaryProviderId is Guid primaryId)
        {
            var decision = await RouteProviderPriorityAsync(
                request, analysis, policy, primaryId, strategy, cancellationToken);
            await FinishAsync(request, decision, cancellationToken);
            return decision;
        }

        var weights = routingPolicy.GetWeights(policy, strategy);
        var candidates = (await providerSelector.SelectProvidersAsync(
            request.OrganizationId,
            analysis,
            cancellationToken)).ToList();

        if (candidates.Count == 0)
        {
            var empty = new RoutingDecision
            {
                Strategy = strategy,
                TaskType = analysis.TaskType,
                Complexity = analysis.Complexity,
                EstimatedInputTokens = analysis.EstimatedInputTokens,
                EstimatedOutputTokens = analysis.EstimatedOutputTokens,
                PolicyId = policy?.Id,
                DecisionReason = "No eligible provider models matched the request capabilities.",
                IsSimulation = request.IsSimulation,
            };
            await FinishAsync(request, empty, cancellationToken);
            return empty;
        }

        foreach (var candidate in candidates)
        {
            var cost = await costEstimator.EstimateAsync(
                candidate,
                analysis.EstimatedInputTokens,
                analysis.EstimatedOutputTokens,
                request.OrganizationId,
                cancellationToken);
            var latency = await latencyPredictor.PredictAsync(
                request.OrganizationId,
                candidate.ProviderId,
                candidate.ModelName,
                cancellationToken);
            candidate.PredictedCostUsd = cost.TotalCostUsd;
            candidate.PredictedLatencyMs = latency.PredictedLatencyMs;
            candidate.AvailabilityScore = await availabilityScorer.ScoreAsync(
                request.OrganizationId,
                candidate.ProviderId,
                cancellationToken);
        }

        var scored = ModelRouter.ScoreCandidates(candidates, analysis, weights);
        var selected = await modelRouter.SelectModelAsync(scored, analysis, weights, cancellationToken);
        var ordered = scored.ToList();
        var fallbacks = ordered.Where(c => selected is null || c.ModelId != selected.ModelId).Take(5).ToList();

        var decisionResult = new RoutingDecision
        {
            Selected = selected,
            Fallbacks = fallbacks,
            ScoredCandidates = ordered,
            Strategy = strategy,
            TaskType = analysis.TaskType,
            Complexity = analysis.Complexity,
            EstimatedInputTokens = analysis.EstimatedInputTokens,
            EstimatedOutputTokens = analysis.EstimatedOutputTokens,
            EstimatedCostUsd = selected?.PredictedCostUsd ?? 0,
            EstimatedLatencyMs = selected?.PredictedLatencyMs ?? 0,
            PolicyId = policy?.Id,
            DecisionReason = selected is null
                ? "No candidate could be selected."
                : $"Selected {selected.ModelName} on {selected.ProviderName} via {strategy} (score {selected.OverallScore:F1}).",
            FallbackCount = 0,
            IsSimulation = request.IsSimulation,
        };

        logger.LogInformation(
            "Routing decision org={OrganizationId} strategy={Strategy} task={TaskType} model={Model} provider={ProviderId} cost={Cost} latency={Latency} simulation={Simulation}",
            request.OrganizationId,
            strategy,
            analysis.TaskType,
            selected?.ModelName,
            selected?.ProviderId,
            decisionResult.EstimatedCostUsd,
            decisionResult.EstimatedLatencyMs,
            request.IsSimulation);

        await FinishAsync(request, decisionResult, cancellationToken);
        return decisionResult;
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

    private async Task<RoutingDecision> RouteProviderPriorityAsync(
        RoutingEngineRequest request,
        RoutingRequestAnalysis analysis,
        AiRoutingPolicy policy,
        Guid primaryProviderId,
        RoutingStrategy strategy,
        CancellationToken cancellationToken)
    {
        var all = await providerSelector.SelectProvidersAsync(request.OrganizationId, analysis, cancellationToken);
        var primary = all.Where(c => c.ProviderId == primaryProviderId).ToList();
        var fallbackIds = ParseGuidList(policy.FallbackProviderIdsJson);
        var ordered = new List<RoutingCandidate>();
        ordered.AddRange(primary);

        foreach (var fallbackId in fallbackIds)
        {
            ordered.AddRange(all.Where(c => c.ProviderId == fallbackId && ordered.All(o => o.ModelId != c.ModelId)));
        }

        if (ordered.Count == 0)
        {
            ordered.AddRange(all);
        }

        foreach (var candidate in ordered)
        {
            var cost = await costEstimator.EstimateAsync(
                candidate,
                analysis.EstimatedInputTokens,
                analysis.EstimatedOutputTokens,
                request.OrganizationId,
                cancellationToken);
            var latency = await latencyPredictor.PredictAsync(
                request.OrganizationId,
                candidate.ProviderId,
                candidate.ModelName,
                cancellationToken);
            candidate.PredictedCostUsd = cost.TotalCostUsd;
            candidate.PredictedLatencyMs = latency.PredictedLatencyMs;
            candidate.AvailabilityScore = await availabilityScorer.ScoreAsync(
                request.OrganizationId,
                candidate.ProviderId,
                cancellationToken);
            candidate.OverallScore = 100 - (ordered.IndexOf(candidate) * 5);
        }

        var selected = ordered.FirstOrDefault();
        return new RoutingDecision
        {
            Selected = selected,
            Fallbacks = ordered.Skip(1).Take(5).ToList(),
            ScoredCandidates = ordered,
            Strategy = strategy,
            TaskType = analysis.TaskType,
            Complexity = analysis.Complexity,
            EstimatedInputTokens = analysis.EstimatedInputTokens,
            EstimatedOutputTokens = analysis.EstimatedOutputTokens,
            EstimatedCostUsd = selected?.PredictedCostUsd ?? 0,
            EstimatedLatencyMs = selected?.PredictedLatencyMs ?? 0,
            PolicyId = policy.Id,
            DecisionReason = selected is null
                ? "ProviderPriority policy had no matching models."
                : $"ProviderPriority selected {selected.ModelName} on {selected.ProviderName}.",
            IsSimulation = request.IsSimulation,
        };
    }

    private async Task FinishAsync(
        RoutingEngineRequest request,
        RoutingDecision decision,
        CancellationToken cancellationToken)
    {
        await PersistDecisionAsync(request.OrganizationId, decision, request.GatewayRequestId, cancellationToken);
        await notificationService.NotifyRoutingDecisionAsync(request.OrganizationId, decision, cancellationToken);
    }

    private static IReadOnlyList<Guid> ParseGuidList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
