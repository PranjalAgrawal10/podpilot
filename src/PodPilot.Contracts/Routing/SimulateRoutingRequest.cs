namespace PodPilot.Contracts.Routing;

/// <summary>
/// Request to simulate intelligent routing.
/// </summary>
public sealed class SimulateRoutingRequest
{
    /// <summary>Gets or sets the prompt to classify and route.</summary>
    public string Prompt { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional strategy override.</summary>
    public string? Strategy { get; init; }

    /// <summary>Gets or sets an optional model hint.</summary>
    public string? ModelHint { get; init; }

    /// <summary>Gets or sets an optional path hint (e.g. /v1/chat/completions).</summary>
    public string? Path { get; init; }
}
