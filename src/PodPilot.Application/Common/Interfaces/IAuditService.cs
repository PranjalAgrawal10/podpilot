using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Service for writing audit log entries.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an auditable action.
    /// </summary>
    /// <param name="action">The action performed.</param>
    /// <param name="entityType">The affected entity type.</param>
    /// <param name="entityId">The affected entity identifier.</param>
    /// <param name="details">Optional details.</param>
    /// <param name="userId">The acting user identifier.</param>
    /// <param name="ipAddress">The client IP address.</param>
    /// <param name="correlationId">The request correlation identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task LogAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string? details = null,
        Guid? userId = null,
        string? ipAddress = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}
