using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Orchestrates intelligent routing by composing classifiers, planners, and persistence.
/// </summary>
public sealed class RoutingEngine : IRoutingEngine
{
    private readonly ITaskClassifier taskClassifier;
    private readonly IRoutingPolicy routingPolicy;
    private readonly IEnumerable<IRoutePlanner> routePlanners;
    private readonly IRoutingDecisionStore decisionStore;
    private readonly IRoutingNotificationService notificationService;
    private readonly ILogger<RoutingEngine> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingEngine"/> class.
    /// </summary>
    public RoutingEngine(
        ITaskClassifier taskClassifier,
        IRoutingPolicy routingPolicy,
        IEnumerable<IRoutePlanner> routePlanners,
        IRoutingDecisionStore decisionStore,
        IRoutingNotificationService notificationService,
        ILogger<RoutingEngine> logger)
    {
        this.taskClassifier = taskClassifier;
        this.routingPolicy = routingPolicy;
        this.routePlanners = routePlanners;
        this.decisionStore = decisionStore;
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
        var analysis = AnalyzeRequest(request);
        var policy = await routingPolicy.GetActivePolicyAsync(
            request.OrganizationId,
            analysis.RequestedModel,
            cancellationToken);

        var strategy = request.StrategyOverride
                       ?? policy?.Strategy
                       ?? RoutingStrategy.Balanced;

        var planner = routePlanners.FirstOrDefault(p => p.CanHandle(strategy, policy))
                      ?? throw new InvalidOperationException($"No route planner registered for strategy '{strategy}'.");

        var decision = await planner.PlanAsync(
            new RoutingPlanContext
            {
                Request = request,
                Analysis = analysis,
                Policy = policy,
                Strategy = strategy,
            },
            cancellationToken);

        logger.LogInformation(
            "Routing decision org={OrganizationId} strategy={Strategy} task={TaskType} model={Model} provider={ProviderId} cost={Cost} latency={Latency} simulation={Simulation}",
            request.OrganizationId,
            strategy,
            analysis.TaskType,
            decision.Selected?.ModelName,
            decision.Selected?.ProviderId,
            decision.EstimatedCostUsd,
            decision.EstimatedLatencyMs,
            request.IsSimulation);

        await decisionStore.PersistDecisionAsync(
            request.OrganizationId,
            decision,
            request.GatewayRequestId,
            cancellationToken);
        await notificationService.NotifyRoutingDecisionAsync(
            request.OrganizationId,
            decision,
            cancellationToken);

        return decision;
    }

    /// <inheritdoc />
    public Task PersistDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        Guid? gatewayRequestId = null,
        CancellationToken cancellationToken = default) =>
        decisionStore.PersistDecisionAsync(organizationId, decision, gatewayRequestId, cancellationToken);

    /// <inheritdoc />
    public Task RecordOutcomeAsync(
        Guid organizationId,
        Guid providerId,
        string? modelName,
        int latencyMs,
        int inputTokens,
        int outputTokens,
        decimal? actualCostUsd = null,
        bool wasColdStart = false,
        CancellationToken cancellationToken = default) =>
        decisionStore.RecordOutcomeAsync(
            organizationId,
            providerId,
            modelName,
            latencyMs,
            inputTokens,
            outputTokens,
            actualCostUsd,
            wasColdStart,
            cancellationToken);

    private RoutingRequestAnalysis AnalyzeRequest(RoutingEngineRequest request)
    {
        var analysis = taskClassifier.Analyze(request.Path, request.BodyJson, request.Prompt);
        if (string.IsNullOrWhiteSpace(request.ModelHint))
        {
            return analysis;
        }

        return new RoutingRequestAnalysis
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
}
