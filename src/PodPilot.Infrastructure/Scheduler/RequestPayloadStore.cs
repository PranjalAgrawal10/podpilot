using System.Collections.Concurrent;
using PodPilot.Application.Models.Scheduler;

namespace PodPilot.Infrastructure.Scheduler;

/// <summary>
/// Temporary in-memory store for queued request payloads awaiting worker dispatch.
/// </summary>
internal static class RequestPayloadStore
{
    private static readonly ConcurrentDictionary<Guid, ScheduleRequestContext> Payloads = new();

    public static void Store(Guid requestId, ScheduleRequestContext context) =>
        Payloads[requestId] = context;

    public static bool TryTake(Guid requestId, out ScheduleRequestContext? context) =>
        Payloads.TryRemove(requestId, out context);

    public static void Remove(Guid requestId) => Payloads.TryRemove(requestId, out _);
}
