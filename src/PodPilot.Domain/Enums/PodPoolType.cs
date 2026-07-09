namespace PodPilot.Domain.Enums;

/// <summary>
/// Classification of a pod pool.
/// </summary>
public enum PodPoolType
{
    /// <summary>Development workloads.</summary>
    Development = 0,

    /// <summary>Production workloads.</summary>
    Production = 1,

    /// <summary>Testing workloads.</summary>
    Testing = 2,

    /// <summary>Custom pool configuration.</summary>
    Custom = 3,
}
