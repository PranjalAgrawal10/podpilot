using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Gateway;

/// <summary>
/// Maps gateway entities to contract responses.
/// </summary>
internal static class GatewayMapper
{
    /// <summary>
    /// Maps a gateway API key to a response.
    /// </summary>
    public static GatewayApiKeyResponse ToApiKeyResponse(GatewayApiKey entity, string? plaintextKey = null) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            KeyPrefix = entity.KeyPrefix,
            KeyType = entity.KeyType.ToString(),
            IsRevoked = entity.IsRevoked,
            ExpiresAt = entity.ExpiresAt,
            RateLimitPerMinute = entity.RateLimitPerMinute,
            RateLimitPerDay = entity.RateLimitPerDay,
            PlaintextKey = plaintextKey,
            CreatedAt = entity.CreatedAt,
        };

    /// <summary>
    /// Maps a gateway route to a response.
    /// </summary>
    public static GatewayRouteResponse ToRouteResponse(GatewayRoute route, string podName) =>
        new()
        {
            Id = route.Id,
            GpuPodId = route.GpuPodId,
            PodName = podName,
            ModelName = route.ModelName,
            IsDefault = route.IsDefault,
        };

    /// <summary>
    /// Maps a gateway request to a summary response.
    /// </summary>
    public static GatewayRequestSummaryResponse ToRequestSummary(GatewayRequest request) =>
        new()
        {
            Id = request.Id,
            GpuPodId = request.GpuPodId,
            Path = request.Path,
            Model = request.Model,
            Status = request.Status.ToString(),
            WakeTriggered = request.WakeTriggered,
            IsStreaming = request.IsStreaming,
            TotalLatencyMs = request.Latency?.TotalLatencyMs,
            StartedAt = request.StartedAt,
            CompletedAt = request.CompletedAt,
        };
}
