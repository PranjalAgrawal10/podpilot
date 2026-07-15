using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// No-op AI provider notifications for the Testing environment.
/// </summary>
public sealed class NoOpAiProviderNotificationService : IAiProviderNotificationService
{
    /// <inheritdoc />
    public Task NotifyProviderConnectedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyProviderDisconnectedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyProviderHealthChangedAsync(
        Guid organizationId,
        Guid providerId,
        string status,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task NotifyModelCatalogUpdatedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
