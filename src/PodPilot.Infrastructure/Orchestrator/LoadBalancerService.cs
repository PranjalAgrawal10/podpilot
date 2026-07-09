using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using StackExchange.Redis;

namespace PodPilot.Infrastructure.Orchestrator;

/// <summary>
/// Selects pods from pools using configurable load balancing strategies.
/// </summary>
public sealed class LoadBalancerService : ILoadBalancer
{
    private readonly IApplicationDbContext dbContext;
    private readonly IDateTimeService dateTimeService;
    private readonly IConnectionMultiplexer? redis;
    private readonly ILogger<LoadBalancerService> logger;
    private readonly Random random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadBalancerService"/> class.
    /// </summary>
    public LoadBalancerService(
        IApplicationDbContext dbContext,
        IDateTimeService dateTimeService,
        ILogger<LoadBalancerService> logger,
        IConnectionMultiplexer? redis = null)
    {
        this.dbContext = dbContext;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
        this.redis = redis;
    }

    /// <inheritdoc />
    public async Task<LoadBalancerSelection?> SelectPodAsync(
        LoadBalancerRequest request,
        CancellationToken cancellationToken = default)
    {
        var eligible = request.Members
            .Where(m => !m.IsWarmStandby)
            .Where(m => m.CurrentLoad < ApplicationConstants.SchedulerMaxConcurrentPerPod)
            .ToList();

        if (eligible.Count == 0)
        {
            eligible = request.Members
                .Where(m => m.CurrentLoad < ApplicationConstants.SchedulerMaxConcurrentPerPod)
                .ToList();
        }

        if (eligible.Count == 0)
        {
            return null;
        }

        var config = await GetEntityConfigAsync(request.OrganizationId, cancellationToken);
        var strategy = config.Strategy;

        if (config.StickySessionsEnabled && !string.IsNullOrWhiteSpace(request.SessionKey))
        {
            var sticky = await TryGetStickySelectionAsync(
                request.OrganizationId,
                request.SessionKey!,
                eligible,
                config.StickySessionTtlMinutes,
                cancellationToken);

            if (sticky is not null)
            {
                return sticky;
            }
        }

        if (strategy == LoadBalancingStrategy.StickySession && !string.IsNullOrWhiteSpace(request.SessionKey))
        {
            var sticky = await TryGetStickySelectionAsync(
                request.OrganizationId,
                request.SessionKey!,
                eligible,
                config.StickySessionTtlMinutes,
                cancellationToken);

            if (sticky is not null)
            {
                return sticky;
            }
        }

        var selected = strategy switch
        {
            LoadBalancingStrategy.RoundRobin => await SelectRoundRobinAsync(request, eligible, cancellationToken),
            LoadBalancingStrategy.LeastBusy => SelectLeastBusy(eligible),
            LoadBalancingStrategy.LeastQueue => SelectLeastQueue(eligible),
            LoadBalancingStrategy.LowestLatency => SelectLowestLatency(eligible),
            LoadBalancingStrategy.Weighted => SelectWeighted(eligible),
            LoadBalancingStrategy.StickySession => SelectLeastBusy(eligible),
            _ => SelectLeastBusy(eligible),
        };

        if (selected is null)
        {
            return null;
        }

        if ((strategy == LoadBalancingStrategy.StickySession || config.StickySessionsEnabled)
            && !string.IsNullOrWhiteSpace(request.SessionKey))
        {
            await StoreStickySessionAsync(
                request.OrganizationId,
                request.SessionKey!,
                selected.PodId,
                config.StickySessionTtlMinutes,
                cancellationToken);
        }

        return new LoadBalancerSelection
        {
            PodId = selected.PodId,
            BaseUrl = selected.BaseUrl,
            CurrentLoad = selected.CurrentLoad,
            Strategy = strategy,
        };
    }

    /// <inheritdoc />
    public async Task<LoadBalancerConfigDto> GetConfigAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var config = await GetEntityConfigAsync(organizationId, cancellationToken);
        return ToDto(config);
    }

    /// <inheritdoc />
    public async Task<LoadBalancerConfigDto> UpdateConfigAsync(
        Guid organizationId,
        LoadBalancerConfigDto config,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.LoadBalancerConfigs
            .FirstOrDefaultAsync(c => c.OrganizationId == organizationId, cancellationToken);

        if (existing is null)
        {
            existing = new LoadBalancerConfig
            {
                OrganizationId = organizationId,
                CreatedAt = dateTimeService.UtcNow,
            };
            await dbContext.AddLoadBalancerConfigAsync(existing, cancellationToken);
        }

        existing.Strategy = config.Strategy;
        existing.StickySessionsEnabled = config.StickySessionsEnabled;
        existing.StickySessionTtlMinutes = config.StickySessionTtlMinutes;
        existing.UpdatedAt = dateTimeService.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(existing);
    }

    private async Task<LoadBalancerConfig> GetEntityConfigAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var config = await dbContext.LoadBalancerConfigs
            .FirstOrDefaultAsync(c => c.OrganizationId == organizationId, cancellationToken);

        return config ?? new LoadBalancerConfig
        {
            OrganizationId = organizationId,
            Strategy = LoadBalancingStrategy.LeastBusy,
            StickySessionTtlMinutes = 30,
        };
    }

    private async Task<PoolMemberContext?> SelectRoundRobinAsync(
        LoadBalancerRequest request,
        IReadOnlyList<PoolMemberContext> eligible,
        CancellationToken cancellationToken)
    {
        if (eligible.Count == 1)
        {
            return eligible[0];
        }

        if (redis is not null)
        {
            var db = redis.GetDatabase();
            var key = OrchestratorRedisKeys.RoundRobinIndex(request.OrganizationId, request.PoolId);
            var index = (int)(await db.StringIncrementAsync(key) % eligible.Count);
            return eligible[index];
        }

        var tick = Environment.TickCount64;
        return eligible[(int)(tick % eligible.Count)];
    }

    private static PoolMemberContext SelectLeastBusy(IReadOnlyList<PoolMemberContext> eligible) =>
        eligible.OrderBy(m => m.CurrentLoad).ThenBy(m => m.QueueDepth).First();

    private static PoolMemberContext SelectLeastQueue(IReadOnlyList<PoolMemberContext> eligible) =>
        eligible.OrderBy(m => m.QueueDepth).ThenBy(m => m.CurrentLoad).First();

    private static PoolMemberContext SelectLowestLatency(IReadOnlyList<PoolMemberContext> eligible) =>
        eligible.OrderBy(m => m.AverageLatencyMs).ThenBy(m => m.CurrentLoad).First();

    private PoolMemberContext SelectWeighted(IReadOnlyList<PoolMemberContext> eligible)
    {
        var totalWeight = eligible.Sum(m => Math.Max(1, m.Weight));
        var roll = random.Next(totalWeight);
        var cumulative = 0;

        foreach (var member in eligible)
        {
            cumulative += Math.Max(1, member.Weight);
            if (roll < cumulative)
            {
                return member;
            }
        }

        return eligible[^1];
    }

    private async Task<LoadBalancerSelection?> TryGetStickySelectionAsync(
        Guid organizationId,
        string sessionKey,
        IReadOnlyList<PoolMemberContext> eligible,
        int ttlMinutes,
        CancellationToken cancellationToken)
    {
        Guid? stickyPodId = null;

        if (redis is not null)
        {
            var db = redis.GetDatabase();
            var value = await db.StringGetAsync(OrchestratorRedisKeys.StickySession(organizationId, sessionKey));
            if (value.HasValue && Guid.TryParse(value.ToString(), out var parsed))
            {
                stickyPodId = parsed;
            }
        }

        if (!stickyPodId.HasValue)
        {
            return null;
        }

        var member = eligible.FirstOrDefault(m => m.PodId == stickyPodId.Value);
        if (member is null)
        {
            return null;
        }

        return new LoadBalancerSelection
        {
            PodId = member.PodId,
            BaseUrl = member.BaseUrl,
            CurrentLoad = member.CurrentLoad,
            Strategy = LoadBalancingStrategy.StickySession,
        };
    }

    private async Task StoreStickySessionAsync(
        Guid organizationId,
        string sessionKey,
        Guid podId,
        int ttlMinutes,
        CancellationToken cancellationToken)
    {
        if (redis is null)
        {
            return;
        }

        var db = redis.GetDatabase();
        await db.StringSetAsync(
            OrchestratorRedisKeys.StickySession(organizationId, sessionKey),
            podId.ToString(),
            TimeSpan.FromMinutes(Math.Max(1, ttlMinutes)));

        logger.LogDebug(
            "Stored sticky session {SessionKey} -> pod {PodId} for organization {OrganizationId}",
            sessionKey,
            podId,
            organizationId);
    }

    private static LoadBalancerConfigDto ToDto(LoadBalancerConfig config) =>
        new()
        {
            Strategy = config.Strategy,
            StickySessionsEnabled = config.StickySessionsEnabled,
            StickySessionTtlMinutes = config.StickySessionTtlMinutes,
        };
}
