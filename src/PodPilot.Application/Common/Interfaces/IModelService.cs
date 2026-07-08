using PodPilot.Contracts.Models;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Orchestrates Ollama model operations on GPU pods.
/// </summary>
public interface IModelService
{
    /// <summary>
    /// Ensures the pod is running and Ollama is reachable.
    /// </summary>
    Task<GpuPod> EnsurePodReadyAsync(
        Guid organizationId,
        Guid podId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Ollama base URL for a pod.
    /// </summary>
    string GetOllamaBaseUrl(GpuPod pod);

    /// <summary>
    /// Starts pulling a model and returns the created download record.
    /// </summary>
    Task<ModelDownloadResponse> StartPullAsync(
        Guid organizationId,
        Guid podId,
        string modelReference,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a model pull in the background.
    /// </summary>
    Task ExecutePullAsync(Guid downloadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a model from Ollama and the database.
    /// </summary>
    Task DeleteModelAsync(
        Guid organizationId,
        Guid modelId,
        bool forceDefault,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the default model for a pod.
    /// </summary>
    Task<ModelResponse> SetDefaultModelAsync(
        Guid organizationId,
        Guid modelId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes models from Ollama tags for a pod.
    /// </summary>
    Task<IReadOnlyList<ModelResponse>> RefreshModelsAsync(
        Guid organizationId,
        Guid podId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to detect Ollama on a running pod without waking it.
    /// </summary>
    Task<(bool Detected, string? Version)> TryDetectOllamaAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default);
}
