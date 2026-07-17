using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.Deployments;

/// <summary>
/// Broadcasts deployment events via SignalR.
/// </summary>
public sealed class DeploymentNotificationService : IDeploymentNotificationService
{
    private readonly IHubContext<DeploymentHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentNotificationService"/> class.
    /// </summary>
    public DeploymentNotificationService(IHubContext<DeploymentHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyStartedAsync(
        Guid organizationId,
        Guid deploymentId,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(DeploymentHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "DeploymentStarted",
                new { deploymentId, updatedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyProgressAsync(
        Guid organizationId,
        Guid deploymentId,
        DeploymentStatus status,
        int progressPercent,
        string? message,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(DeploymentHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "DeploymentProgress",
                new { deploymentId, status = status.ToString(), progressPercent, message, updatedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyModelProgressAsync(
        Guid organizationId,
        Guid deploymentId,
        string modelReference,
        int progressPercent,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(DeploymentHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "DeploymentModelProgress",
                new { deploymentId, modelReference, progressPercent, updatedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyHealthAsync(
        Guid organizationId,
        Guid deploymentId,
        DeploymentHealthState state,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(DeploymentHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "DeploymentHealth",
                new { deploymentId, state = state.ToString(), updatedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyReadyAsync(
        Guid organizationId,
        Guid deploymentId,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(DeploymentHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "DeploymentReady",
                new { deploymentId, updatedAt = DateTime.UtcNow },
                cancellationToken);

    /// <inheritdoc />
    public Task NotifyFailedAsync(
        Guid organizationId,
        Guid deploymentId,
        string error,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(DeploymentHub.GetOrganizationGroupName(organizationId))
            .SendAsync(
                "DeploymentFailed",
                new { deploymentId, error, updatedAt = DateTime.UtcNow },
                cancellationToken);
}

/// <summary>
/// No-op deployment notifications for Testing.
/// </summary>
public sealed class NoOpDeploymentNotificationService : IDeploymentNotificationService
{
    /// <inheritdoc />
    public Task NotifyStartedAsync(
        Guid organizationId,
        Guid deploymentId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyProgressAsync(
        Guid organizationId,
        Guid deploymentId,
        DeploymentStatus status,
        int progressPercent,
        string? message,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyModelProgressAsync(
        Guid organizationId,
        Guid deploymentId,
        string modelReference,
        int progressPercent,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyHealthAsync(
        Guid organizationId,
        Guid deploymentId,
        DeploymentHealthState state,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyReadyAsync(
        Guid organizationId,
        Guid deploymentId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyFailedAsync(
        Guid organizationId,
        Guid deploymentId,
        string error,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
