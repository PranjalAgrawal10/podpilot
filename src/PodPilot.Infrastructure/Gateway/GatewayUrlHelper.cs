using PodPilot.Domain.Entities;
using PodPilot.Infrastructure.Ollama;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Helpers for building gateway upstream URLs.
/// </summary>
internal static class GatewayUrlHelper
{
    /// <summary>
    /// Gets the Ollama base URL for a GPU pod.
    /// </summary>
    public static string GetOllamaBaseUrl(GpuPod pod) => OllamaUrlHelper.GetOllamaBaseUrl(pod);

    /// <summary>
    /// Combines a base URL and path.
    /// </summary>
    public static string Combine(string baseUrl, string path) => OllamaUrlHelper.Combine(baseUrl, path);
}
