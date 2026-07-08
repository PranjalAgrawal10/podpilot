namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Client for inference backends (Ollama).
/// </summary>
public interface IInferenceClient
{
    /// <summary>
    /// Waits until the inference backend is healthy.
    /// </summary>
    /// <param name="baseUrl">Ollama base URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="maxAttempts">Maximum health check attempts. Uses the default when null.</param>
    /// <param name="checkInterval">Delay between attempts. Uses the default when null.</param>
    /// <param name="requestTimeout">Per-request timeout. Uses the default when null.</param>
    Task<bool> WaitForHealthyAsync(
        string baseUrl,
        CancellationToken cancellationToken = default,
        int? maxAttempts = null,
        TimeSpan? checkInterval = null,
        TimeSpan? requestTimeout = null);

    /// <summary>
    /// Checks if the inference backend is healthy.
    /// </summary>
    /// <param name="baseUrl">Ollama base URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="requestTimeout">Per-request timeout. Uses the default when null.</param>
    Task<bool> IsHealthyAsync(
        string baseUrl,
        CancellationToken cancellationToken = default,
        TimeSpan? requestTimeout = null);

    /// <summary>
    /// Lists models from the inference backend.
    /// </summary>
    Task<string> GetModelsAsync(
        string baseUrl,
        CancellationToken cancellationToken = default);
}
