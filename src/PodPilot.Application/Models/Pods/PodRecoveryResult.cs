namespace PodPilot.Application.Models.Pods;

/// <summary>
/// Result of attempting to replace a failed pod with a new provider instance.
/// </summary>
public sealed class PodRecoveryResult
{
    /// <summary>
    /// Gets or sets a value indicating whether replacement succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets an optional error message when replacement fails.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets the provider pod information after replacement.
    /// </summary>
    public PodInfo? Pod { get; init; }
}
