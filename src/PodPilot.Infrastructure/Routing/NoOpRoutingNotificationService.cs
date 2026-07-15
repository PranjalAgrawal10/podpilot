using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// No-op routing notifications for testing.
/// </summary>
public sealed class NoOpRoutingNotificationService : IRoutingNotificationService
{
    /// <inheritdoc />
    public Task NotifyRoutingDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyProviderChangedAsync(
        Guid organizationId,
        Guid? previousProviderId,
        Guid providerId,
        string modelName,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyFallbackOccurredAsync(
        Guid organizationId,
        Guid fromProviderId,
        Guid? toProviderId,
        string? modelName,
        string reason,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyPolicyUpdatedAsync(
        Guid organizationId,
        Guid policyId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
