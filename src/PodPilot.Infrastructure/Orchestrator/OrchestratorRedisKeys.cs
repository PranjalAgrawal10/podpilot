namespace PodPilot.Infrastructure.Orchestrator;

/// <summary>
/// Redis key prefixes for orchestration data structures.
/// </summary>
internal static class OrchestratorRedisKeys
{
    public const string RoundRobinPrefix = "orchestrator:rr:";
    public const string StickySessionPrefix = "orchestrator:sticky:";

    /// <summary>
    /// Gets the round-robin index key for a pool.
    /// </summary>
    public static string RoundRobinIndex(Guid organizationId, Guid poolId) =>
        $"{RoundRobinPrefix}{organizationId}:{poolId}";

    /// <summary>
    /// Gets the sticky session key for a client session.
    /// </summary>
    public static string StickySession(Guid organizationId, string sessionKey) =>
        $"{StickySessionPrefix}{organizationId}:{sessionKey}";
}
