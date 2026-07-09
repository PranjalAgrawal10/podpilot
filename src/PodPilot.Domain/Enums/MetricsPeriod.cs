namespace PodPilot.Domain.Enums;

/// <summary>
/// Time period for metrics and cost aggregation.
/// </summary>
public enum MetricsPeriod
{
    /// <summary>Hourly aggregation.</summary>
    Hourly = 0,

    /// <summary>Daily aggregation.</summary>
    Daily = 1,

    /// <summary>Weekly aggregation.</summary>
    Weekly = 2,

    /// <summary>Monthly aggregation.</summary>
    Monthly = 3,
}
