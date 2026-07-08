using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Pods;
using PodPilot.Contracts.Gateway;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Gateway.Queries.GetGatewayStats;

/// <summary>
/// Handles gateway statistics queries.
/// </summary>
public sealed class GetGatewayStatsQueryHandler : IRequestHandler<GetGatewayStatsQuery, GatewayStatsResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGatewayStatsQueryHandler"/> class.
    /// </summary>
    public GetGatewayStatsQueryHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<GatewayStatsResponse> Handle(GetGatewayStatsQuery request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.GatewayRead,
            cancellationToken);

        var oneHourAgo = dateTimeService.UtcNow.AddHours(-1);
        var requests = await dbContext.GatewayRequests
            .Where(r => r.OrganizationId == organizationId)
            .Include(r => r.Latency)
            .ToListAsync(cancellationToken);

        var activeStatuses = new[]
        {
            GatewayRequestStatus.Pending,
            GatewayRequestStatus.Waking,
            GatewayRequestStatus.WaitingHealthy,
            GatewayRequestStatus.Forwarding,
            GatewayRequestStatus.Streaming,
        };

        var activeRequests = requests.Count(r => activeStatuses.Contains(r.Status));
        var streamingRequests = requests.Count(r => r.Status == GatewayRequestStatus.Streaming);
        var waitingPods = requests.Count(r =>
            r.Status == GatewayRequestStatus.Waking || r.Status == GatewayRequestStatus.WaitingHealthy);

        var completedLatencies = requests
            .Where(r => r.Latency is not null)
            .Select(r => r.Latency!.TotalLatencyMs)
            .ToList();

        return new GatewayStatsResponse
        {
            ActiveRequests = activeRequests,
            StreamingRequests = streamingRequests,
            WaitingPods = waitingPods,
            AverageLatencyMs = completedLatencies.Count == 0
                ? 0
                : completedLatencies.Average(),
            RecentErrors = requests.Count(r =>
                r.Status == GatewayRequestStatus.Failed && r.StartedAt >= oneHourAgo),
        };
    }
}
