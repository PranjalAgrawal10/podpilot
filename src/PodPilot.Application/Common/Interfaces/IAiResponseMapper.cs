using PodPilot.Application.Models.AiProviders;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Normalizes provider responses into OpenAI-compatible payloads.
/// </summary>
public interface IAiResponseMapper
{
    /// <summary>
    /// Serializes an internal chat response to OpenAI chat completion JSON.
    /// </summary>
    string ToOpenAiChatCompletionJson(AiChatResponse response);

    /// <summary>
    /// Serializes an internal embedding response to OpenAI embeddings JSON.
    /// </summary>
    string ToOpenAiEmbeddingsJson(AiEmbeddingResponse response);

    /// <summary>
    /// Writes an OpenAI-compatible streaming chunk to the response stream.
    /// </summary>
    Task WriteOpenAiStreamChunkAsync(
        Stream responseStream,
        string model,
        string? contentDelta,
        bool isDone,
        CancellationToken cancellationToken = default);
}
