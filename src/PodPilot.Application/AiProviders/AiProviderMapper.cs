using System.Text.Json;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.AiProviders;

/// <summary>
/// Maps AI provider entities to contract responses.
/// </summary>
internal static class AiProviderMapper
{
    /// <summary>
    /// Maps an AI inference provider entity to a response DTO.
    /// </summary>
    public static AiProviderResponse ToResponse(AiInferenceProvider provider) =>
        new()
        {
            Id = provider.Id,
            OrganizationId = provider.OrganizationId,
            Name = provider.Name,
            DisplayName = provider.DisplayName,
            Description = provider.Description,
            ProviderKind = provider.ProviderKind.ToString(),
            BaseUrl = provider.BaseUrl,
            DeploymentName = provider.DeploymentName,
            ApiVersion = provider.ApiVersion,
            IsEnabled = provider.IsEnabled,
            IsValidated = provider.IsValidated,
            LastValidatedAt = provider.LastValidatedAt,
            Priority = provider.Priority,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt,
        };

    /// <summary>
    /// Maps an AI provider model entity to a response DTO.
    /// </summary>
    public static AiProviderModelResponse ToModelResponse(AiProviderModel model) =>
        new()
        {
            Id = model.Id,
            OrganizationId = model.OrganizationId,
            AiProviderId = model.AiProviderId,
            ProviderKind = model.AiProvider?.ProviderKind.ToString() ?? string.Empty,
            ProviderDisplayName = model.AiProvider?.DisplayName ?? string.Empty,
            ModelName = model.ModelName,
            DisplayName = model.DisplayName,
            ContextLength = model.ContextLength,
            Parameters = model.Parameters,
            SupportsStreaming = model.SupportsStreaming,
            SupportsVision = model.SupportsVision,
            SupportsTools = model.SupportsTools,
            SupportsEmbeddings = model.SupportsEmbeddings,
            InputCostPerMillionTokens = model.InputCostPerMillionTokens,
            OutputCostPerMillionTokens = model.OutputCostPerMillionTokens,
            IsEnabled = model.IsEnabled,
            SyncedAt = model.SyncedAt,
        };

    /// <summary>
    /// Maps AI provider health to a response DTO.
    /// </summary>
    public static AiProviderHealthResponse ToHealthResponse(AiProviderHealth health) =>
        new()
        {
            ProviderId = health.AiProviderId,
            Status = health.Status.ToString(),
            LatencyMs = health.LatencyMs,
            ErrorRate = health.ErrorRate,
            ErrorMessage = health.ErrorMessage,
            LastCheckedAt = health.LastCheckedAt,
            ConsecutiveFailures = health.ConsecutiveFailures,
        };

    /// <summary>
    /// Maps a routing policy entity to a response DTO.
    /// </summary>
    public static AiRoutingPolicyResponse ToRoutingPolicyResponse(AiRoutingPolicy policy) =>
        new()
        {
            Id = policy.Id,
            OrganizationId = policy.OrganizationId,
            Name = policy.Name,
            ModelName = policy.ModelName,
            PrimaryProviderId = policy.PrimaryProviderId,
            PrimaryProviderDisplayName = policy.PrimaryProvider?.DisplayName,
            FallbackProviderIds = ParseFallbackIds(policy.FallbackProviderIdsJson),
            FailoverStrategy = policy.FailoverStrategy.ToString(),
            Strategy = policy.Strategy.ToString(),
            MaxRetries = policy.MaxRetries,
            IsEnabled = policy.IsEnabled,
            IsDefault = policy.IsDefault,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt,
        };

    /// <summary>
    /// Maps dashboard metrics to a response DTO.
    /// </summary>
    public static AiProviderDashboardResponse ToDashboardResponse(AiProviderDashboard dashboard) =>
        new()
        {
            ConnectedProviders = dashboard.ConnectedProviders,
            TotalProviders = dashboard.TotalProviders,
            AvailableModels = dashboard.AvailableModels,
            UnhealthyProviders = dashboard.UnhealthyProviders,
            StreamingSessions = dashboard.StreamingSessions,
            AverageLatencyMs = dashboard.AverageLatencyMs,
            AverageErrorRate = dashboard.AverageErrorRate,
        };

    /// <summary>
    /// Maps provider kind metadata to a response DTO.
    /// </summary>
    public static AiProviderKindMetadataResponse ToKindMetadataResponse(AiProviderKindMetadata metadata) =>
        new()
        {
            ProviderKind = metadata.ProviderKind.ToString(),
            DisplayName = metadata.DisplayName,
            DefaultBaseUrl = metadata.DefaultBaseUrl,
            RequiresBaseUrl = metadata.RequiresBaseUrl,
            RequiresApiKey = metadata.RequiresApiKey,
            IsOpenAiCompatible = metadata.IsOpenAiCompatible,
        };

    /// <summary>
    /// Serializes fallback provider IDs to JSON.
    /// </summary>
    public static string ToFallbackJson(IReadOnlyList<Guid> ids) =>
        JsonSerializer.Serialize(ids ?? Array.Empty<Guid>());

    private static IReadOnlyList<Guid> ParseFallbackIds(string json)
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
