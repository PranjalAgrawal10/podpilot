using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Abstraction for an AI inference provider.
/// </summary>
public interface IAiProvider
{
    /// <summary>Gets the provider kind handled by this implementation.</summary>
    AiProviderKind ProviderKind { get; }

    /// <summary>Lists available models.</summary>
    Task<IReadOnlyList<AiModelInfo>> ListModelsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default);

    /// <summary>Performs a chat completion.</summary>
    Task<AiChatResponse> ChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Performs a streaming chat completion.</summary>
    Task StreamChatAsync(
        AiProviderConnection connection,
        AiChatRequest request,
        Stream responseStream,
        CancellationToken cancellationToken = default);

    /// <summary>Creates embeddings.</summary>
    Task<AiEmbeddingResponse> EmbeddingsAsync(
        AiProviderConnection connection,
        AiEmbeddingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Checks provider health.</summary>
    Task<AiProviderHealthResult> HealthAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default);

    /// <summary>Validates credentials.</summary>
    Task<AiCredentialValidationResult> ValidateCredentialsAsync(
        AiProviderConnection connection,
        CancellationToken cancellationToken = default);

    /// <summary>Estimates token count when supported.</summary>
    Task<int?> CountTokensAsync(
        AiProviderConnection connection,
        string model,
        string text,
        CancellationToken cancellationToken = default);
}
