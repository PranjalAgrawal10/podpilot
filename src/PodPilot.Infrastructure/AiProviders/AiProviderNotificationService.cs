using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Hubs;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Broadcasts AI provider events via SignalR.
/// </summary>
public sealed class AiProviderNotificationService : IAiProviderNotificationService
{
    private readonly IHubContext<AiProviderHub> hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiProviderNotificationService"/> class.
    /// </summary>
    public AiProviderNotificationService(IHubContext<AiProviderHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    /// <inheritdoc />
    public Task NotifyProviderConnectedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "AiProviderConnected", new { providerId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyProviderDisconnectedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "AiProviderDisconnected", new { providerId }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyProviderHealthChangedAsync(
        Guid organizationId,
        Guid providerId,
        string status,
        CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "AiProviderHealthChanged", new { providerId, status }, cancellationToken);

    /// <inheritdoc />
    public Task NotifyModelCatalogUpdatedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default) =>
        SendAsync(organizationId, "AiModelCatalogUpdated", new { providerId }, cancellationToken);

    private Task SendAsync(
        Guid organizationId,
        string eventName,
        object payload,
        CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(AiProviderHub.GetOrganizationGroupName(organizationId))
            .SendAsync(eventName, new { payload, updatedAt = DateTime.UtcNow }, cancellationToken);
}
