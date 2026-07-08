namespace PodPilot.Domain.Enums;

/// <summary>
/// Result of a model health check.
/// </summary>
public enum ModelHealthStatus
{
    /// <summary>Ollama is running and the model responded successfully.</summary>
    Healthy = 0,

    /// <summary>Ollama is running but the model check failed.</summary>
    Unhealthy = 1,

    /// <summary>Ollama is not reachable on the pod.</summary>
    Unavailable = 2,

    /// <summary>Model is not available on the pod.</summary>
    ModelMissing = 3,
}
