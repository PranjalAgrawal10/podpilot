using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Persistence;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Persists audit log entries.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly ApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    public AuditService(ApplicationDbContext dbContext, IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task LogAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string? details = null,
        Guid? userId = null,
        string? ipAddress = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            UserId = userId,
            IpAddress = ipAddress,
            CorrelationId = correlationId,
            Timestamp = dateTimeService.UtcNow,
        };

        await dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
