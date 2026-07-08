using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Helpers for building gateway upstream URLs.
/// </summary>
internal static class GatewayUrlHelper
{
    /// <summary>
    /// Gets the Ollama base URL for a GPU pod.
    /// </summary>
    public static string GetOllamaBaseUrl(GpuPod pod)
    {
        if (!string.IsNullOrWhiteSpace(pod.Endpoint))
        {
            if (Uri.TryCreate(pod.Endpoint, UriKind.Absolute, out var endpointUri))
            {
                var port = endpointUri.Port > 0 ? endpointUri.Port : 11434;
                return $"{endpointUri.Scheme}://{endpointUri.Host}:{port}";
            }
        }

        if (!string.IsNullOrWhiteSpace(pod.PublicIp))
        {
            return $"http://{pod.PublicIp}:11434";
        }

        throw new InvalidOperationException($"Pod '{pod.Name}' has no reachable endpoint for gateway forwarding.");
    }

    /// <summary>
    /// Combines a base URL and path.
    /// </summary>
    public static string Combine(string baseUrl, string path)
    {
        var normalizedBase = baseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        return $"{normalizedBase}{normalizedPath}";
    }
}
