namespace PodPilot.Domain.Entities;

/// <summary>
/// Latency metrics for a gateway request.
/// </summary>
public class GatewayLatency
{
    /// <summary>
    /// Gets or sets the gateway request identifier.
    /// </summary>
    public Guid GatewayRequestId { get; set; }

    /// <summary>
    /// Gets or sets wake latency in milliseconds.
    /// </summary>
    public int? WakeLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets health check latency in milliseconds.
    /// </summary>
    public int? HealthCheckLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets forward latency in milliseconds.
    /// </summary>
    public int? ForwardLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets total latency in milliseconds.
    /// </summary>
    public int TotalLatencyMs { get; set; }

    /// <summary>
    /// Gets the gateway request.
    /// </summary>
    public GatewayRequest Request { get; set; } = null!;
}
