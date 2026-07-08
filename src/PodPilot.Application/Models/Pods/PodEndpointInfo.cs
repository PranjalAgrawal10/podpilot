namespace PodPilot.Application.Models.Pods;

/// <summary>
/// Endpoint information for a provider pod.
/// </summary>
public sealed class PodEndpointInfo
{
    /// <summary>
    /// Gets or sets the internal port.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Gets or sets the protocol.
    /// </summary>
    public string Protocol { get; init; } = "http";

    /// <summary>
    /// Gets or sets the mapped public port.
    /// </summary>
    public int? PublicPort { get; init; }

    /// <summary>
    /// Gets or sets the public URL.
    /// </summary>
    public string? Url { get; init; }
}
