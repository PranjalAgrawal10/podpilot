using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Broadcasts intelligent routing events via SignalR.
/// </summary>
public sealed class RoutingNotificationService : IRoutingNotificationService
{
    private readonly IHubContext<RoutingHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingNotificationService"/> class.
    /// </summary>
    public RoutingNotificationService(IHubContext<RoutingHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyRoutingDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            organizationId,
            "RoutingDecision",
            new
            {
                strategy = decision.Strategy.ToString(),
                taskType = decision.TaskType.ToString(),
                model = decision.Selected?.ModelName,
                providerId = decision.Selected?.ProviderId,
                providerName = decision.Selected?.ProviderName,
                estimatedCostUsd = decision.EstimatedCostUsd,
                estimatedLatencyMs = decision.EstimatedLatencyMs,
                overallScore = decision.Selected?.OverallScore,
                isSimulation = decision.IsSimulation,
                reason = decision.DecisionReason,
            },
            cancellationToken);

    /// <inheritdoc />
    public Task NotifyProviderChangedAsync(
        Guid organizationId,
        Guid? previousProviderId,
        Guid providerId,
        string modelName,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            organizationId,
            "ProviderChanged",
            new { previousProviderId, providerId, modelName },
            cancellationToken);

    /// <inheritdoc />
    public Task NotifyFallbackOccurredAsync(
        Guid organizationId,
        Guid fromProviderId,
        Guid? toProviderId,
        string? modelName,
        string reason,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            organizationId,
            "FallbackOccurred",
            new { fromProviderId, toProviderId, modelName, reason },
            cancellationToken);

    /// <inheritdoc />
    public Task NotifyPolicyUpdatedAsync(
        Guid organizationId,
        Guid policyId,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "PolicyUpdated", new { policyId }, cancellationToken);

    private Task SendAsync(Guid organizationId, string method, object payload, CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(RoutingHub.GetOrganizationGroupName(organizationId))
            .SendAsync(method, payload, cancellationToken);
}
