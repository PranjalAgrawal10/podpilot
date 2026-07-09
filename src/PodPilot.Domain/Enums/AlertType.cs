namespace PodPilot.Domain.Enums;

/// <summary>
/// Types of observability alerts.
/// </summary>
public enum AlertType
{
    /// <summary>GPU utilization exceeds threshold.</summary>
    HighGpuUsage = 0,

    /// <summary>Request queue length exceeds threshold.</summary>
    HighQueueLength = 1,

    /// <summary>Latency exceeds threshold.</summary>
    HighLatency = 2,

    /// <summary>A pod has failed.</summary>
    PodFailure = 3,

    /// <summary>A provider has failed.</summary>
    ProviderFailure = 4,

    /// <summary>Disk usage exceeds threshold.</summary>
    DiskFull = 5,

    /// <summary>Memory usage exceeds threshold.</summary>
    MemoryPressure = 6,

    /// <summary>A model health check failed.</summary>
    ModelFailure = 7,

    /// <summary>Repeated gateway errors detected.</summary>
    RepeatedGatewayErrors = 8,
}
