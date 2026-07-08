using PodPilot.Application.Models.Ollama;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// HTTP client abstraction for Ollama APIs on GPU pods.
/// </summary>
public interface IOllamaClient
{
    /// <summary>
    /// Gets the Ollama server version.
    /// </summary>
    Task<OllamaVersionResult> GetVersionAsync(string baseUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists installed model tags.
    /// </summary>
    Task<IReadOnlyList<OllamaModelTag>> ListModelsAsync(string baseUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists installed model tags (alias for ListModels).
    /// </summary>
    Task<IReadOnlyList<OllamaModelTag>> GetTagsAsync(string baseUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows detailed metadata for a model.
    /// </summary>
    Task<OllamaModelDetails> ShowModelAsync(
        string baseUrl,
        string modelName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulls a model with streaming progress updates.
    /// </summary>
    Task PullModelAsync(
        string baseUrl,
        string modelName,
        Func<OllamaPullProgress, Task> onProgress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a model from Ollama.
    /// </summary>
    Task DeleteModelAsync(string baseUrl, string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a completion for health checks.
    /// </summary>
    Task<OllamaGenerateResult> GenerateAsync(
        string baseUrl,
        string modelName,
        string prompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates embeddings for a model.
    /// </summary>
    Task<OllamaEmbeddingsResult> EmbeddingsAsync(
        string baseUrl,
        string modelName,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether Ollama is reachable on the pod.
    /// </summary>
    Task<bool> IsReachableAsync(string baseUrl, CancellationToken cancellationToken = default);
}
