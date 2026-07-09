using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Orchestration.Commands.UpdatePodPool;

/// <summary>
/// Handles pod pool updates.
/// </summary>
public sealed class UpdatePodPoolCommandHandler : IRequestHandler<UpdatePodPoolCommand, PodPoolResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodPoolManager podPoolManager;
    private readonly IOrchestratorNotificationService notificationService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePodPoolCommandHandler"/> class.
    /// </summary>
    public UpdatePodPoolCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodPoolManager podPoolManager,
        IOrchestratorNotificationService notificationService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.podPoolManager = podPoolManager;
        this.notificationService = notificationService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public async Task<PodPoolResponse> Handle(UpdatePodPoolCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorManage,
            cancellationToken);

        var pool = await OrchestrationAccess.GetPodPoolAsync(
            dbContext,
            request.PoolId,
            organizationId,
            cancellationToken,
            includeDetails: true);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedName = request.Name.Trim();
            var nameExists = await dbContext.PodPools.AnyAsync(
                p => p.OrganizationId == organizationId && p.Name == normalizedName && p.Id != pool.Id,
                cancellationToken);

            if (nameExists)
            {
                throw new ValidationException(
                [
                    new FluentValidation.Results.ValidationFailure(nameof(request.Name), "A pool with this name already exists."),
                ]);
            }

            pool.Name = normalizedName;
        }

        if (request.Description is not null)
        {
            pool.Description = request.Description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.PoolType))
        {
            pool.PoolType = OrchestrationMapper.ParsePoolType(request.PoolType);
        }

        if (request.IsDefault == true)
        {
            var existingDefaults = await dbContext.PodPools
                .Where(p => p.OrganizationId == organizationId && p.IsDefault && p.Id != pool.Id)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }

            pool.IsDefault = true;
        }
        else if (request.IsDefault == false)
        {
            pool.IsDefault = false;
        }

        if (request.IsActive.HasValue)
        {
            pool.IsActive = request.IsActive.Value;
        }

        if (request.ScalingPolicy is not null)
        {
            if (pool.ScalingPolicy is null)
            {
                var policy = new ScalingPolicy
                {
                    OrganizationId = organizationId,
                    Name = request.ScalingPolicy.Name.Trim(),
                    MinPods = request.ScalingPolicy.MinPods,
                    MaxPods = request.ScalingPolicy.MaxPods,
                    MaxQueueLength = request.ScalingPolicy.MaxQueueLength,
                    MaxLatencyMs = request.ScalingPolicy.MaxLatencyMs,
                    ScaleUpThreshold = request.ScalingPolicy.ScaleUpThreshold,
                    ScaleDownThreshold = request.ScalingPolicy.ScaleDownThreshold,
                    WarmStandbyCount = request.ScalingPolicy.WarmStandbyCount,
                    MinRuntimeMinutes = request.ScalingPolicy.MinRuntimeMinutes,
                    AutoScaleUpEnabled = request.ScalingPolicy.AutoScaleUpEnabled,
                    AutoScaleDownEnabled = request.ScalingPolicy.AutoScaleDownEnabled,
                    CreatedBy = userId.ToString(),
                };

                await dbContext.AddScalingPolicyAsync(policy, cancellationToken);
                pool.ScalingPolicyId = policy.Id;
                pool.ScalingPolicy = policy;
            }
            else
            {
                pool.ScalingPolicy.Name = request.ScalingPolicy.Name.Trim();
                pool.ScalingPolicy.MinPods = request.ScalingPolicy.MinPods;
                pool.ScalingPolicy.MaxPods = request.ScalingPolicy.MaxPods;
                pool.ScalingPolicy.MaxQueueLength = request.ScalingPolicy.MaxQueueLength;
                pool.ScalingPolicy.MaxLatencyMs = request.ScalingPolicy.MaxLatencyMs;
                pool.ScalingPolicy.ScaleUpThreshold = request.ScalingPolicy.ScaleUpThreshold;
                pool.ScalingPolicy.ScaleDownThreshold = request.ScalingPolicy.ScaleDownThreshold;
                pool.ScalingPolicy.WarmStandbyCount = request.ScalingPolicy.WarmStandbyCount;
                pool.ScalingPolicy.MinRuntimeMinutes = request.ScalingPolicy.MinRuntimeMinutes;
                pool.ScalingPolicy.AutoScaleUpEnabled = request.ScalingPolicy.AutoScaleUpEnabled;
                pool.ScalingPolicy.AutoScaleDownEnabled = request.ScalingPolicy.AutoScaleDownEnabled;
                pool.ScalingPolicy.UpdatedAt = dateTimeService.UtcNow;
                pool.ScalingPolicy.UpdatedBy = userId.ToString();
            }
        }

        if (request.Models is not null)
        {
            await dbContext.RemovePodPoolModelsAsync(pool.Id, cancellationToken);

            foreach (var modelName in request.Models)
            {
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    continue;
                }

                await dbContext.AddPodPoolModelAsync(
                    new PodPoolModel { PodPoolId = pool.Id, ModelName = modelName.Trim() },
                    cancellationToken);
            }
        }

        pool.UpdatedAt = dateTimeService.UtcNow;
        pool.UpdatedBy = userId.ToString();
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.PodIds is not null)
        {
            var existingMemberIds = pool.Members.Select(m => m.GpuPodId).ToHashSet();
            var requestedIds = request.PodIds.ToHashSet();

            foreach (var member in pool.Members.Where(m => !requestedIds.Contains(m.GpuPodId)).ToList())
            {
                await podPoolManager.RemoveMemberAsync(organizationId, pool.Id, member.GpuPodId, cancellationToken);
            }

            foreach (var podId in requestedIds.Where(id => !existingMemberIds.Contains(id)))
            {
                await podPoolManager.AddMemberAsync(organizationId, pool.Id, podId, cancellationToken: cancellationToken);
            }
        }

        await auditService.LogAsync(
            AuditAction.Updated,
            nameof(PodPool),
            pool.Id.ToString(),
            $"Pod pool '{pool.Name}' updated",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await notificationService.NotifyPoolUpdatedAsync(organizationId, pool.Id, cancellationToken);

        var updated = await OrchestrationAccess.GetPodPoolAsync(
            dbContext,
            pool.Id,
            organizationId,
            cancellationToken,
            includeDetails: true);

        return OrchestrationMapper.ToResponse(updated);
    }
}
