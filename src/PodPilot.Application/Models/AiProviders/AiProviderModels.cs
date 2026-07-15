using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.AiProviders;

/// <summary>
/// Connection details for invoking an AI provider.
/// </summary>
public sealed class AiProviderConnection
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the provider configuration identifier.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Gets or sets the provider kind.</summary>
    public AiProviderKind ProviderKind { get; init; }

    /// <summary>Gets or sets the decrypted API key.</summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Gets or sets the base URL.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional deployment name.</summary>
    public string? DeploymentName { get; init; }

    /// <summary>Gets or sets an optional API version.</summary>
    public string? ApiVersion { get; init; }
}

/// <summary>
/// Metadata describing a provider kind.
/// </summary>
public sealed class AiProviderKindMetadata
{
    /// <summary>Gets or sets the provider kind.</summary>
    public AiProviderKind ProviderKind { get; init; }

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets or sets the default base URL.</summary>
    public string DefaultBaseUrl { get; init; } = string.Empty;

    /// <summary>Gets or sets whether a custom base URL is required.</summary>
    public bool RequiresBaseUrl { get; init; }

    /// <summary>Gets or sets whether an API key is required.</summary>
    public bool RequiresApiKey { get; init; } = true;

    /// <summary>Gets or sets whether the provider is OpenAI-compatible.</summary>
    public bool IsOpenAiCompatible { get; init; } = true;
}

/// <summary>
/// Model catalog entry returned by a provider.
/// </summary>
public sealed class AiModelInfo
{
    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Gets or sets the context length.</summary>
    public int? ContextLength { get; init; }

    /// <summary>Gets or sets the parameter label.</summary>
    public string? Parameters { get; init; }

    /// <summary>Gets or sets whether streaming is supported.</summary>
    public bool SupportsStreaming { get; init; } = true;

    /// <summary>Gets or sets whether vision is supported.</summary>
    public bool SupportsVision { get; init; }

    /// <summary>Gets or sets whether tools are supported.</summary>
    public bool SupportsTools { get; init; }

    /// <summary>Gets or sets whether embeddings are supported.</summary>
    public bool SupportsEmbeddings { get; init; }

    /// <summary>Gets or sets optional input cost per 1M tokens.</summary>
    public decimal? InputCostPerMillionTokens { get; init; }

    /// <summary>Gets or sets optional output cost per 1M tokens.</summary>
    public decimal? OutputCostPerMillionTokens { get; init; }
}

/// <summary>
/// Internal chat message.
/// </summary>
public sealed class AiChatMessage
{
    /// <summary>Gets or sets the role.</summary>
    public string Role { get; init; } = "user";

    /// <summary>Gets or sets the content.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional name.</summary>
    public string? Name { get; init; }
}

/// <summary>
/// Internal chat request.
/// </summary>
public sealed class AiChatRequest
{
    /// <summary>Gets or sets the model name.</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Gets or sets the messages.</summary>
    public IReadOnlyList<AiChatMessage> Messages { get; init; } = [];

    /// <summary>Gets or sets temperature.</summary>
    public double? Temperature { get; init; }

    /// <summary>Gets or sets max tokens.</summary>
    public int? MaxTokens { get; init; }

    /// <summary>Gets or sets top-p.</summary>
    public double? TopP { get; init; }

    /// <summary>Gets or sets whether streaming is requested.</summary>
    public bool Stream { get; init; }

    /// <summary>Gets or sets an optional system prompt.</summary>
    public string? SystemPrompt { get; init; }

    /// <summary>Gets or sets stop sequences.</summary>
    public IReadOnlyList<string>? Stop { get; init; }
}

/// <summary>
/// Internal chat response.
/// </summary>
public sealed class AiChatResponse
{
    /// <summary>Gets or sets the response identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets or sets the model name.</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Gets or sets the assistant content.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Gets or sets the finish reason.</summary>
    public string? FinishReason { get; init; }

    /// <summary>Gets or sets prompt token count.</summary>
    public int? PromptTokens { get; init; }

    /// <summary>Gets or sets completion token count.</summary>
    public int? CompletionTokens { get; init; }

    /// <summary>Gets or sets total token count.</summary>
    public int? TotalTokens { get; init; }

    /// <summary>Gets or sets the provider kind that produced the response.</summary>
    public AiProviderKind ProviderKind { get; init; }
}

/// <summary>
/// Internal embedding request.
/// </summary>
public sealed class AiEmbeddingRequest
{
    /// <summary>Gets or sets the model name.</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Gets or sets input texts.</summary>
    public IReadOnlyList<string> Input { get; init; } = [];
}

/// <summary>
/// Internal embedding response.
/// </summary>
public sealed class AiEmbeddingResponse
{
    /// <summary>Gets or sets the model name.</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Gets or sets embedding vectors.</summary>
    public IReadOnlyList<IReadOnlyList<float>> Embeddings { get; init; } = [];

    /// <summary>Gets or sets total tokens.</summary>
    public int? TotalTokens { get; init; }
}

/// <summary>
/// Health check result for an AI provider.
/// </summary>
public sealed class AiProviderHealthResult
{
    /// <summary>Gets or sets a value indicating whether the provider is healthy.</summary>
    public bool IsHealthy { get; init; }

    /// <summary>Gets or sets latency in milliseconds.</summary>
    public int? LatencyMs { get; init; }

    /// <summary>Gets or sets an optional message.</summary>
    public string? Message { get; init; }
}

/// <summary>
/// Credential validation result.
/// </summary>
public sealed class AiCredentialValidationResult
{
    /// <summary>Gets or sets a value indicating whether credentials are valid.</summary>
    public bool IsValid { get; init; }

    /// <summary>Gets or sets an optional message.</summary>
    public string? Message { get; init; }
}

/// <summary>
/// Resolved AI inference route.
/// </summary>
public sealed class AiInferenceRoute
{
    /// <summary>Gets or sets the primary connection.</summary>
    public AiProviderConnection Connection { get; init; } = null!;

    /// <summary>Gets or sets the model name.</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Gets or sets fallback connections in order.</summary>
    public IReadOnlyList<AiProviderConnection> FallbackConnections { get; init; } = [];

    /// <summary>Gets or sets the failover strategy.</summary>
    public AiFailoverStrategy FailoverStrategy { get; init; }

    /// <summary>Gets or sets max retries on the primary provider.</summary>
    public int MaxRetries { get; init; } = 2;

    /// <summary>Gets or sets the routing policy identifier when applicable.</summary>
    public Guid? RoutingPolicyId { get; init; }
}

/// <summary>
/// Dashboard summary for AI providers.
/// </summary>
public sealed class AiProviderDashboard
{
    /// <summary>Gets or sets connected (healthy) provider count.</summary>
    public int ConnectedProviders { get; init; }

    /// <summary>Gets or sets total enabled providers.</summary>
    public int TotalProviders { get; init; }

    /// <summary>Gets or sets available model count.</summary>
    public int AvailableModels { get; init; }

    /// <summary>Gets or sets unhealthy provider count.</summary>
    public int UnhealthyProviders { get; init; }

    /// <summary>Gets or sets active streaming sessions estimate.</summary>
    public int StreamingSessions { get; init; }

    /// <summary>Gets or sets average provider latency in milliseconds.</summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>Gets or sets average provider error rate.</summary>
    public double AverageErrorRate { get; init; }
}
