using PodPilot.Domain.Entities;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Performs health checks against Ollama models.
/// </summary>
public interface IModelHealthService
{
    /// <summary>
    /// Checks health for a single model.
    /// </summary>
    Task<ModelHealthHistory> CheckModelHealthAsync(
        AiModel model,
        GpuPod pod,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks health for all available models on a pod.
    /// </summary>
    Task CheckPodModelsAsync(GpuPod pod, CancellationToken cancellationToken = default);
}
