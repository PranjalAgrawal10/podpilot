using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Records AI provider failover events.
/// </summary>
public sealed class AiFailoverService : IAiFailoverService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiFailoverService"/> class.
    /// </summary>
    public AiFailoverService(IApplicationDbContext dbContext, IDateTimeService dateTimeService)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task RecordFailoverAsync(
        Guid organizationId,
        Guid fromProviderId,
        Guid? toProviderId,
        string? modelName,
        string reason,
        bool succeeded,
        Guid? gatewayRequestId = null,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AddAiFailoverEventAsync(
            new AiFailoverEvent
            {
                OrganizationId = organizationId,
                OccurredAt = dateTimeService.UtcNow,
                ModelName = modelName,
                FromProviderId = fromProviderId,
                ToProviderId = toProviderId,
                Reason = reason.Length > 2000 ? reason[..2000] : reason,
                GatewayRequestId = gatewayRequestId,
                Succeeded = succeeded,
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
