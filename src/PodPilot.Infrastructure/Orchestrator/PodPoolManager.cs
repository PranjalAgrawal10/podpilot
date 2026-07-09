using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Gateway;
using PodPilot.Infrastructure.Scheduler;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Orchestrator;

/// <summary>
/// Manages pod pool membership and resolution.
/// </summary>
public sealed class PodPoolManager : IPodPoolManager
{
    private readonly IApplicationDbContext dbContext;
    private readonly IOrchestratorNotificationService notificationService;
    private readonly IDateTimeService dateTimeService;
    private readonly IConnectionMultiplexer? redis;
    private readonly ILogger<PodPoolManager> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodPoolManager"/> class.
    /// </summary>
    public PodPoolManager(
        IApplicationDbContext dbContext,
        IOrchestratorNotificationService notificationService,
        IDateTimeService dateTimeService,
        ILogger<PodPoolManager> logger,
        IConnectionMultiplexer? redis = null)
    {
        this.dbContext = dbContext;
        this.notificationService = notificationService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<PodPool?> ResolvePoolAsync(
        Guid organizationId,
        string? modelName,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(modelName))
        {
            var normalizedModel = modelName.Trim();
            var modelPool = await dbContext.PodPools
                .Where(p => p.OrganizationId == organizationId && p.IsActive)
                .Where(p => p.Models.Any(m => m.ModelName == normalizedModel))
                .OrderByDescending(p => p.IsDefault)
                .ThenBy(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (modelPool is not null)
            {
                return modelPool;
            }
        }

        var defaultPool = await dbContext.PodPools
            .Where(p => p.OrganizationId == organizationId && p.IsActive && p.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultPool is not null)
        {
            return defaultPool;
        }

        return await dbContext.PodPools
            .Where(p => p.OrganizationId == organizationId && p.IsActive)
            .OrderBy(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PoolMemberContext>> GetHealthyMembersAsync(
        Guid poolId,
        CancellationToken cancellationToken = default)
    {
        var members = await dbContext.PodPoolMembers
            .Where(m => m.PodPoolId == poolId)
            .Where(m => m.State == OrchestrationPodState.Healthy || m.State == OrchestrationPodState.Busy)
            .Include(m => m.GpuPod)
            .ToListAsync(cancellationToken);

        var contexts = new List<PoolMemberContext>();

        foreach (var member in members)
        {
            var pod = member.GpuPod;
            if (pod.Status == PodStatus.Deleted || pod.Status == PodStatus.Deleting)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(pod.Endpoint) && string.IsNullOrWhiteSpace(pod.PublicIp))
            {
                continue;
            }

            var organizationId = pod.OrganizationId;
            var currentLoad = await GetPodLoadAsync(organizationId, pod.Id, member, cancellationToken);
            var queueDepth = await GetQueueDepthAsync(organizationId, pod.Id, cancellationToken);
            var averageLatencyMs = await GetAverageLatencyAsync(organizationId, pod.Id, cancellationToken);

            contexts.Add(new PoolMemberContext
            {
                MemberId = member.Id,
                PodId = pod.Id,
                Pod = pod,
                BaseUrl = GatewayUrlHelper.GetOllamaBaseUrl(pod),
                State = member.State,
                Weight = member.Weight,
                CurrentLoad = currentLoad,
                QueueDepth = queueDepth,
                AverageLatencyMs = averageLatencyMs,
                AffinityTag = member.AffinityTag,
                IsWarmStandby = member.IsWarmStandby,
            });
        }

        return contexts;
    }

    /// <inheritdoc />
    public async Task<PodPoolMember> AddMemberAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        bool isWarmStandby = false,
        CancellationToken cancellationToken = default)
    {
        var pool = await dbContext.PodPools
            .FirstOrDefaultAsync(p => p.Id == poolId && p.OrganizationId == organizationId, cancellationToken)
            ?? throw new InvalidOperationException($"Pod pool {poolId} was not found.");

        var pod = await dbContext.GpuPods
            .FirstOrDefaultAsync(
                p => p.Id == podId && p.OrganizationId == organizationId && p.Status != PodStatus.Deleted,
                cancellationToken)
            ?? throw new InvalidOperationException($"GPU pod {podId} was not found.");

        var existing = await dbContext.PodPoolMembers
            .FirstOrDefaultAsync(m => m.PodPoolId == poolId && m.GpuPodId == podId, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var member = new PodPoolMember
        {
            PodPoolId = poolId,
            GpuPodId = podId,
            State = ResolveInitialState(pod),
            IsWarmStandby = isWarmStandby,
            JoinedAt = dateTimeService.UtcNow,
        };

        await dbContext.AddPodPoolMemberAsync(member, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.NotifyPodAddedAsync(organizationId, poolId, podId, cancellationToken);

        logger.LogInformation(
            "Added pod {PodId} to pool {PoolId} for organization {OrganizationId}",
            podId,
            poolId,
            organizationId);

        return member;
    }

    /// <inheritdoc />
    public async Task RemoveMemberAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        CancellationToken cancellationToken = default)
    {
        var member = await dbContext.PodPoolMembers
            .Where(m => m.PodPoolId == poolId && m.GpuPodId == podId)
            .Join(
                dbContext.PodPools.Where(p => p.OrganizationId == organizationId),
                member => member.PodPoolId,
                pool => pool.Id,
                (member, _) => member)
            .FirstOrDefaultAsync(cancellationToken);

        if (member is null)
        {
            return;
        }

        await dbContext.RemovePodPoolMemberAsync(member.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.NotifyPodRemovedAsync(organizationId, poolId, podId, cancellationToken);

        logger.LogInformation(
            "Removed pod {PodId} from pool {PoolId} for organization {OrganizationId}",
            podId,
            poolId,
            organizationId);
    }

    /// <inheritdoc />
    public async Task UpdateMemberStateAsync(
        Guid poolId,
        Guid podId,
        OrchestrationPodState state,
        CancellationToken cancellationToken = default)
    {
        var member = await dbContext.PodPoolMembers
            .FirstOrDefaultAsync(m => m.PodPoolId == poolId && m.GpuPodId == podId, cancellationToken);

        if (member is null)
        {
            return;
        }

        member.State = state;
        if (state == OrchestrationPodState.Draining && member.DrainingStartedAt is null)
        {
            member.DrainingStartedAt = dateTimeService.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task StartDrainingAsync(
        Guid organizationId,
        Guid poolId,
        Guid podId,
        CancellationToken cancellationToken = default)
    {
        var member = await dbContext.PodPoolMembers
            .Where(m => m.PodPoolId == poolId && m.GpuPodId == podId)
            .Join(
                dbContext.PodPools.Where(p => p.OrganizationId == organizationId),
                member => member.PodPoolId,
                pool => pool.Id,
                (member, _) => member)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Pod {podId} is not a member of pool {poolId}.");

        member.State = OrchestrationPodState.Draining;
        member.DrainingStartedAt = dateTimeService.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveStreamsAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken = default)
    {
        var member = await dbContext.PodPoolMembers
            .FirstOrDefaultAsync(m => m.GpuPodId == podId, cancellationToken);

        if (member?.ActiveStreams > 0)
        {
            return true;
        }

        var load = await GetPodLoadAsync(organizationId, podId, member, cancellationToken);
        return load > 0;
    }

    private static OrchestrationPodState ResolveInitialState(GpuPod pod) =>
        pod.Status switch
        {
            PodStatus.Running => OrchestrationPodState.Healthy,
            PodStatus.Stopped or PodStatus.Stopping => OrchestrationPodState.Stopped,
            PodStatus.Failed => OrchestrationPodState.Failed,
            _ => OrchestrationPodState.Provisioning,
        };

    private async Task<int> GetPodLoadAsync(
        Guid organizationId,
        Guid podId,
        PodPoolMember? member,
        CancellationToken cancellationToken)
    {
        if (redis is not null)
        {
            var db = redis.GetDatabase();
            var value = await db.StringGetAsync(SchedulerRedisKeys.PodLoad(organizationId, podId));
            if (value.HasValue && int.TryParse(value.ToString(), out var load))
            {
                return load;
            }
        }

        var activeRequests = await dbContext.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId
                && r.GpuPodId == podId
                && (r.Status == GatewayRequestStatus.Forwarding
                    || r.Status == GatewayRequestStatus.Streaming
                    || r.Status == GatewayRequestStatus.Waking
                    || r.Status == GatewayRequestStatus.WaitingHealthy),
            cancellationToken);

        return Math.Max(activeRequests, member?.ActiveStreams ?? 0);
    }

    private Task<int> GetQueueDepthAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken) =>
        dbContext.GatewayRequests.CountAsync(
            r => r.OrganizationId == organizationId
                && r.GpuPodId == podId
                && r.Status == GatewayRequestStatus.Queued,
            cancellationToken);

    private async Task<int> GetAverageLatencyAsync(
        Guid organizationId,
        Guid podId,
        CancellationToken cancellationToken)
    {
        var cutoff = dateTimeService.UtcNow.AddMinutes(-15);
        var metrics = await dbContext.PodHealthMetrics
            .Where(m => m.OrganizationId == organizationId && m.GpuPodId == podId && m.RecordedAt >= cutoff)
            .OrderByDescending(m => m.RecordedAt)
            .Take(5)
            .Select(m => m.LatencyMs)
            .ToListAsync(cancellationToken);

        return metrics.Count > 0 ? (int)metrics.Average() : 0;
    }
}
