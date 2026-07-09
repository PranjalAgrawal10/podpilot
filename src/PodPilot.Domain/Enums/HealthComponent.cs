namespace PodPilot.Domain.Enums;

/// <summary>
/// Components monitored for health.
/// </summary>
public enum HealthComponent
{
    /// <summary>Overall system health.</summary>
    System = 0,

    /// <summary>Compute provider health.</summary>
    Provider = 1,

    /// <summary>GPU pod health.</summary>
    Pod = 2,

    /// <summary>AI gateway health.</summary>
    Gateway = 3,

    /// <summary>Ollama inference health.</summary>
    Ollama = 4,

    /// <summary>Database health.</summary>
    Database = 5,

    /// <summary>Redis health.</summary>
    Redis = 6,

    /// <summary>SignalR health.</summary>
    SignalR = 7,
}
