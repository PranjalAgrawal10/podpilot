using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Pods;

/// <summary>
/// Result of a pod lifecycle operation.
/// </summary>
public sealed class PodOperationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the resulting pod status.
    /// </summary>
    public PodStatus Status { get; init; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets optional updated pod information.
    /// </summary>
    public PodInfo? Pod { get; init; }
}
