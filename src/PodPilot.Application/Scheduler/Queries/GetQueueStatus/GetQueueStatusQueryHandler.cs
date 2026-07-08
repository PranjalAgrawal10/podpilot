using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Scheduler;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Scheduler.Queries.GetQueueStatus;

/// <summary>
/// Handles <see cref="GetQueueStatusQuery"/>.
/// </summary>
public sealed class GetQueueStatusQueryHandler : IRequestHandler<GetQueueStatusQuery, QueueStatusResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IRequestQueue requestQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetQueueStatusQueryHandler"/> class.
    /// </summary>
    public GetQueueStatusQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IRequestQueue requestQueue)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.requestQueue = requestQueue;
    }

    /// <inheritdoc />
    public async Task<QueueStatusResponse> Handle(GetQueueStatusQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);
        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recent = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId && r.CreatedAt >= oneHourAgo)
            .ToListAsync(cancellationToken);

        return new QueueStatusResponse
        {
            QueueLength = await requestQueue.GetLengthAsync(organizationId, cancellationToken),
            RunningRequests = recent.Count(r => r.Status is GatewayRequestStatus.Forwarding or GatewayRequestStatus.Waking or GatewayRequestStatus.WaitingHealthy),
            StreamingRequests = recent.Count(r => r.Status == GatewayRequestStatus.Streaming),
            FailedRequestsLastHour = recent.Count(r => r.Status == GatewayRequestStatus.Failed),
            AverageWaitTimeMs = recent.Where(r => r.QueueTimeMs.HasValue).Select(r => (double)r.QueueTimeMs!.Value).DefaultIfEmpty(0).Average(),
            AverageExecutionTimeMs = recent.Where(r => r.ExecutionTimeMs.HasValue).Select(r => (double)r.ExecutionTimeMs!.Value).DefaultIfEmpty(0).Average(),
        };
    }
}
