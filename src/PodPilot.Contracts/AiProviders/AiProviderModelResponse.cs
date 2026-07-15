namespace PodPilot.Contracts.AiProviders;

/// <summary>
/// AI provider model catalog entry response.
/// </summary>
public sealed class AiProviderModelResponse
{
    /// <summary>Gets or sets the model entry identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; init; }

    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid AiProviderId { get; init; }

    /// <summary>Gets or sets the provider kind.</summary>
    public string ProviderKind { get; init; } = string.Empty;

    /// <summary>Gets or sets the provider display name.</summary>
    public string ProviderDisplayName { get; init; } = string.Empty;

    /// <summary>Gets or sets the model name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional display name.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Gets or sets the context length.</summary>
    public int? ContextLength { get; init; }

    /// <summary>Gets or sets the parameter label.</summary>
    public string? Parameters { get; init; }

    /// <summary>Gets or sets whether streaming is supported.</summary>
    public bool SupportsStreaming { get; init; }

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

    /// <summary>Gets or sets a value indicating whether the model is enabled.</summary>
    public bool IsEnabled { get; init; }

    /// <summary>Gets or sets when the catalog entry was last synced.</summary>
    public DateTime SyncedAt { get; init; }
}
