namespace PodPilot.Domain.Enums;

/// <summary>
/// Direction of an auto-scaling action.
/// </summary>
public enum ScalingDirection
{
    /// <summary>Scale up by adding pods.</summary>
    ScaleUp = 0,

    /// <summary>Scale down by removing pods.</summary>
    ScaleDown = 1,
}
