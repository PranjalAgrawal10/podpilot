namespace PodPilot.Contracts.Pods;

/// <summary>
/// Pod endpoint response.
/// </summary>
public sealed class PodEndpointResponse
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
    /// Gets or sets the public port.
    /// </summary>
    public int? PublicPort { get; init; }

    /// <summary>
    /// Gets or sets the public URL.
    /// </summary>
    public string? Url { get; init; }
}
