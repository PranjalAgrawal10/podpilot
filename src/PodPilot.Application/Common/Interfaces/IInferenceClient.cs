namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Client for inference backends (Ollama).
/// </summary>
public interface IInferenceClient
{
    /// <summary>
    /// Waits until the inference backend is healthy.
    /// </summary>
    Task<bool> WaitForHealthyAsync(
        string baseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the inference backend is healthy.
    /// </summary>
    Task<bool> IsHealthyAsync(
        string baseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists models from the inference backend.
    /// </summary>
    Task<string> GetModelsAsync(
        string baseUrl,
        CancellationToken cancellationToken = default);
}
