using PodPilot.Application.Models.AiProviders;
using PodPilot.Contracts.AiProviders;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Application service for AI inference provider management.
/// </summary>
public interface IAiProviderService
{
    /// <summary>Syncs the model catalog for a provider.</summary>
    Task SyncModelsAsync(
        Domain.Entities.AiInferenceProvider provider,
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>Checks and persists provider health.</summary>
    Task<AiProviderHealthResponse> CheckHealthAsync(
        Guid organizationId,
        Guid providerId,
        CancellationToken cancellationToken = default);

    /// <summary>Builds a connection for a stored provider.</summary>
    Task<AiProviderConnection> CreateConnectionAsync(
        Domain.Entities.AiInferenceProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>Gets dashboard metrics.</summary>
    Task<AiProviderDashboard> GetDashboardAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves AI inference routes for gateway requests.
/// </summary>
public interface IAiInferenceRouter
{
    /// <summary>
    /// Attempts to resolve an AI provider route for the given model.
    /// Returns null when the request should use the pod/Ollama path.
    /// </summary>
    Task<AiInferenceRoute?> TryResolveAsync(
        Guid organizationId,
        string? model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to resolve an AI provider route using path and body for intelligent classification.
    /// Returns null when the request should use the pod/Ollama path.
    /// </summary>
    Task<AiInferenceRoute?> TryResolveAsync(
        Guid organizationId,
        string? model,
        string? path,
        string? bodyJson,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Dispatches inference through AI providers with failover.
/// </summary>
public interface IAiInferenceDispatcher
{
    /// <summary>Dispatches a chat/embeddings request through the AI provider stack.</summary>
    Task<AiDispatchResult> DispatchAsync(
        AiDispatchContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Failover orchestration for AI providers.
/// </summary>
public interface IAiFailoverService
{
    /// <summary>Records a failover event.</summary>
    Task RecordFailoverAsync(
        Guid organizationId,
        Guid fromProviderId,
        Guid? toProviderId,
        string? modelName,
        string reason,
        bool succeeded,
        Guid? gatewayRequestId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// SignalR notifications for AI providers.
/// </summary>
public interface IAiProviderNotificationService
{
    /// <summary>Notifies that a provider connected.</summary>
    Task NotifyProviderConnectedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that a provider disconnected.</summary>
    Task NotifyProviderDisconnectedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>Notifies that provider health changed.</summary>
    Task NotifyProviderHealthChangedAsync(
        Guid organizationId,
        Guid providerId,
        string status,
        CancellationToken cancellationToken = default);

    /// <summary>Notifies that the model catalog was updated.</summary>
    Task NotifyModelCatalogUpdatedAsync(Guid organizationId, Guid providerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dispatch context for AI provider inference.
/// </summary>
public sealed class AiDispatchContext
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the resolved route.</summary>
    public AiInferenceRoute Route { get; init; } = null!;

    /// <summary>Gets or sets the request path.</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Gets or sets the HTTP method.</summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>Gets or sets the raw request body JSON.</summary>
    public string BodyJson { get; init; } = string.Empty;

    /// <summary>Gets or sets the response stream.</summary>
    public System.IO.Stream ResponseBody { get; init; } = System.IO.Stream.Null;

    /// <summary>Gets or sets optional gateway request id.</summary>
    public Guid? GatewayRequestId { get; init; }

    /// <summary>Gets or sets a value indicating whether streaming was requested.</summary>
    public bool Stream { get; init; }
}

/// <summary>
/// Result of an AI provider dispatch.
/// </summary>
public sealed class AiDispatchResult
{
    /// <summary>Gets or sets a value indicating success.</summary>
    public bool Success { get; init; }

    /// <summary>Gets or sets the HTTP status code.</summary>
    public int StatusCode { get; init; } = 200;

    /// <summary>Gets or sets an optional error message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets or sets the provider that handled the request.</summary>
    public Guid? HandledByProviderId { get; init; }

    /// <summary>Gets or sets a value indicating whether failover occurred.</summary>
    public bool FailoverOccurred { get; init; }
}
