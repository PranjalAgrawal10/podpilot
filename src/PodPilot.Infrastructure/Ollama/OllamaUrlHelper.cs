using PodPilot.Application.Common;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Ollama;

/// <summary>
/// Helpers for building Ollama URLs from GPU pods.
/// </summary>
public static class OllamaUrlHelper
{
    /// <summary>
    /// Gets the Ollama base URL for a GPU pod.
    /// </summary>
    public static string GetOllamaBaseUrl(GpuPod pod)
    {
        var ollamaEndpoint = pod.Endpoints
            .FirstOrDefault(e =>
                e.Port == ApplicationConstants.OllamaPort
                && e.Protocol.Equals("http", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(ollamaEndpoint?.Url))
        {
            return NormalizeBaseUrl(ollamaEndpoint.Url);
        }

        if (ollamaEndpoint?.PublicPort is int publicPort && !string.IsNullOrWhiteSpace(pod.PublicIp))
        {
            return $"http://{pod.PublicIp}:{publicPort}";
        }

        if (!string.IsNullOrWhiteSpace(pod.Endpoint)
            && Uri.TryCreate(pod.Endpoint, UriKind.Absolute, out var endpointUri)
            && endpointUri.Port == ApplicationConstants.OllamaPort)
        {
            return $"{endpointUri.Scheme}://{endpointUri.Host}:{endpointUri.Port}";
        }

        if (!string.IsNullOrWhiteSpace(pod.PublicIp))
        {
            return $"http://{pod.PublicIp}:{ApplicationConstants.OllamaPort}";
        }

        if (!string.IsNullOrWhiteSpace(pod.Endpoint)
            && Uri.TryCreate(pod.Endpoint, UriKind.Absolute, out var fallbackUri))
        {
            return $"{fallbackUri.Scheme}://{fallbackUri.Host}:{ApplicationConstants.OllamaPort}";
        }

        throw new InvalidOperationException($"Pod '{pod.Name}' has no reachable endpoint for Ollama.");
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

    private static string NormalizeBaseUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url.TrimEnd('/');
        }

        var port = uri.Port > 0 ? uri.Port : ApplicationConstants.OllamaPort;
        return $"{uri.Scheme}://{uri.Host}:{port}";
    }
}
