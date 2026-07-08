namespace PodPilot.Contracts.Models;

/// <summary>
/// Request to pull a model from Ollama.
/// </summary>
public sealed class PullModelRequest
{
    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the model reference (e.g. llama3:latest).
    /// </summary>
    public string Model { get; set; } = string.Empty;
}
