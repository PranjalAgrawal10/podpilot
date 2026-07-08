namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Redis key prefixes for scheduler data structures.
/// </summary>
internal static class SchedulerRedisKeys
{
    public const string QueuePrefix = "scheduler:queue:";
    public const string DuplicatePrefix = "scheduler:dup:";
    public const string PodLoadPrefix = "scheduler:podload:";
    public const string LockPrefix = "scheduler:lock:";
    public const string ProcessingPrefix = "scheduler:processing:";

    public static string Queue(Guid organizationId) => $"{QueuePrefix}{organizationId}";

    public static string Duplicate(Guid organizationId, string clientRequestId) =>
        $"{DuplicatePrefix}{organizationId}:{clientRequestId}";

    public static string PodLoad(Guid organizationId, Guid podId) =>
        $"{PodLoadPrefix}{organizationId}:{podId}";

    public static string Lock(string lockKey) => $"{LockPrefix}{lockKey}";

    public static string Processing(Guid requestId) => $"{ProcessingPrefix}{requestId}";
}
