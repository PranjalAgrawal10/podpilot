using PodPilot.Contracts.Scheduler;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Scheduler;

/// <summary>
/// Maps scheduler entities to contract responses.
/// </summary>
public static class SchedulerMapper
{
    /// <summary>
    /// Maps a gateway request to a scheduler response.
    /// </summary>
    public static SchedulerRequestResponse ToResponse(GatewayRequest request) =>
        new()
        {
            Id = request.Id,
            OrganizationId = request.OrganizationId,
            PodId = request.GpuPodId,
            Model = request.Model,
            Path = request.Path,
            Status = request.Status.ToString(),
            Priority = request.Priority.ToString(),
            CreatedAt = request.CreatedAt,
            StartedAt = request.StartedAt,
            CompletedAt = request.CompletedAt,
            QueueTimeMs = request.QueueTimeMs,
            ExecutionTimeMs = request.ExecutionTimeMs,
            RetryCount = request.RetryCount,
            IsStreaming = request.IsStreaming,
        };
}
