namespace PodPilot.Contracts.Models;

/// <summary>
/// Request to refresh models from Ollama.
/// </summary>
public sealed class RefreshModelsRequest
{
    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid PodId { get; set; }
}
