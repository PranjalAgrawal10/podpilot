using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Records an execution attempt for a scheduled request.
/// </summary>
public class RequestExecution : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the linked gateway request identifier.
    /// </summary>
    public Guid GatewayRequestId { get; set; }

    /// <summary>
    /// Gets or sets the assigned pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the attempt number.
    /// </summary>
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Gets or sets the execution status.
    /// </summary>
    public SchedulerRequestStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when execution started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when execution completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
