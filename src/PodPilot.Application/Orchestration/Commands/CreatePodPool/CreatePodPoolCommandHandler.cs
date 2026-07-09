using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Contracts.Orchestration;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Orchestration.Commands.CreatePodPool;

/// <summary>
/// Handles pod pool creation.
/// </summary>
public sealed class CreatePodPoolCommandHandler : IRequestHandler<CreatePodPoolCommand, PodPoolResponse>
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
    /// Initializes a new instance of the <see cref="CreatePodPoolCommandHandler"/> class.
    /// </summary>
    public CreatePodPoolCommandHandler(
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
    public async Task<PodPoolResponse> Handle(CreatePodPoolCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = OrchestrationAccess.RequireOrganizationContext(currentUserService);

        await OrchestrationAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.OrchestratorManage,
            cancellationToken);

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.PodPools.AnyAsync(
            p => p.OrganizationId == organizationId && p.Name == normalizedName,
            cancellationToken);

        if (nameExists)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(request.Name), "A pool with this name already exists."),
            ]);
        }

        if (request.IsDefault)
        {
            var existingDefaults = await dbContext.PodPools
                .Where(p => p.OrganizationId == organizationId && p.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        ScalingPolicy? scalingPolicy = null;
        if (request.ScalingPolicy is not null)
        {
            scalingPolicy = CreateScalingPolicy(organizationId, request.ScalingPolicy, userId);
            await dbContext.AddScalingPolicyAsync(scalingPolicy, cancellationToken);
        }

        GpuType? gpuType = null;
        if (!string.IsNullOrWhiteSpace(request.GpuType)
            && Enum.TryParse<GpuType>(request.GpuType, ignoreCase: true, out var parsedGpuType))
        {
            gpuType = parsedGpuType;
        }

        var pool = new PodPool
        {
            OrganizationId = organizationId,
            Name = normalizedName,
            Description = request.Description?.Trim(),
            PoolType = OrchestrationMapper.ParsePoolType(request.PoolType),
            IsDefault = request.IsDefault,
            ProviderId = request.ProviderId,
            GpuId = request.GpuId,
            GpuType = gpuType,
            Region = request.Region,
            TemplateId = request.TemplateId,
            ImageName = request.ImageName,
            ScalingPolicyId = scalingPolicy?.Id,
            ScalingPolicy = scalingPolicy,
            CreatedAt = dateTimeService.UtcNow,
            CreatedBy = userId.ToString(),
        };

        await dbContext.AddPodPoolAsync(pool, cancellationToken);

        foreach (var modelName in request.Models ?? [])
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                continue;
            }

            await dbContext.AddPodPoolModelAsync(
                new PodPoolModel { PodPoolId = pool.Id, ModelName = modelName.Trim() },
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var podId in request.PodIds ?? [])
        {
            await podPoolManager.AddMemberAsync(organizationId, pool.Id, podId, cancellationToken: cancellationToken);
        }

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(PodPool),
            pool.Id.ToString(),
            $"Pod pool '{pool.Name}' created",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await notificationService.NotifyPoolUpdatedAsync(organizationId, pool.Id, cancellationToken);

        var created = await OrchestrationAccess.GetPodPoolAsync(
            dbContext,
            pool.Id,
            organizationId,
            cancellationToken,
            includeDetails: true);

        return OrchestrationMapper.ToResponse(created);
    }

    private static ScalingPolicy CreateScalingPolicy(Guid organizationId, ScalingPolicyRequest request, Guid userId) =>
        new()
        {
            OrganizationId = organizationId,
            Name = request.Name.Trim(),
            MinPods = request.MinPods,
            MaxPods = request.MaxPods,
            MaxQueueLength = request.MaxQueueLength,
            MaxLatencyMs = request.MaxLatencyMs,
            ScaleUpThreshold = request.ScaleUpThreshold,
            ScaleDownThreshold = request.ScaleDownThreshold,
            WarmStandbyCount = request.WarmStandbyCount,
            MinRuntimeMinutes = request.MinRuntimeMinutes,
            AutoScaleUpEnabled = request.AutoScaleUpEnabled,
            AutoScaleDownEnabled = request.AutoScaleDownEnabled,
            CreatedBy = userId.ToString(),
        };
}
