namespace PodPilot.Infrastructure.Observability;

/// <summary>
/// Redis key prefixes for observability caching.
/// </summary>
internal static class ObservabilityRedisKeys
{
    public const string LiveMetricsPrefix = "observability:live:";

    /// <summary>
    /// Gets the live metrics cache key for an organization.
    /// </summary>
    public static string LiveMetrics(Guid organizationId) =>
        $"{LiveMetricsPrefix}{organizationId}";
}
