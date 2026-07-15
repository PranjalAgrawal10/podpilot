using MediatR;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Pods;
using PodPilot.Contracts.Pods;
using PodPilot.Domain.Authorization;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods.Commands.CreatePod;

/// <summary>
/// Handles GPU pod creation.
/// </summary>
public sealed class CreatePodCommandHandler : IRequestHandler<CreatePodCommand, PodResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IApplicationDbContext dbContext;
    private readonly IPodService podService;
    private readonly IPodNotificationService podNotificationService;
    private readonly IPodLifecycleService podLifecycleService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;
    private readonly IQuotaService quotaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePodCommandHandler"/> class.
    /// </summary>
    public CreatePodCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IApplicationDbContext dbContext,
        IPodService podService,
        IPodNotificationService podNotificationService,
        IPodLifecycleService podLifecycleService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService,
        IQuotaService quotaService)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.dbContext = dbContext;
        this.podService = podService;
        this.podNotificationService = podNotificationService;
        this.podLifecycleService = podLifecycleService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
        this.quotaService = quotaService;
    }

    /// <inheritdoc />
    public async Task<PodResponse> Handle(CreatePodCommand request, CancellationToken cancellationToken)
    {
        var (userId, organizationId) = PodAccess.RequireOrganizationContext(currentUserService);

        await PodAccess.EnsurePermissionAsync(
            organizationAuthorizationService,
            organizationId,
            userId,
            PermissionNames.PodCreate,
            cancellationToken);

        await quotaService.EnsureCanCreatePodAsync(organizationId, cancellationToken);

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.GpuPods.AnyAsync(
            p => p.OrganizationId == organizationId && p.Name == normalizedName && p.Status != PodStatus.Deleted,
            cancellationToken);

        if (nameExists)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Name),
                    "A pod with this name already exists."),
            ]);
        }

        var provider = await PodAccess.GetProviderAsync(
            dbContext,
            request.ProviderId,
            organizationId,
            cancellationToken);

        if (!provider.IsEnabled || !provider.IsValidated)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ProviderId),
                    "Provider must be enabled and validated before creating pods."),
            ]);
        }

        var now = dateTimeService.UtcNow;
        var pod = new GpuPod
        {
            OrganizationId = organizationId,
            ProviderId = provider.Id,
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = PodStatus.BuildingPending,
            GpuType = request.GpuType,
            GpuId = request.GpuId.Trim(),
            Region = request.Region.Trim(),
            TemplateId = string.IsNullOrWhiteSpace(request.TemplateId) ? null : request.TemplateId.Trim(),
            ImageName = request.ImageName.Trim(),
            ContainerDisk = request.ContainerDiskGb,
            VolumeDisk = request.VolumeDiskGb,
            IsPublic = request.EnablePublicIp,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
        };

        var configuration = new PodConfiguration
        {
            TemplateId = pod.TemplateId,
            TemplateName = string.IsNullOrWhiteSpace(request.TemplateName) ? null : request.TemplateName.Trim(),
            ImageName = pod.ImageName,
            ContainerDiskGb = request.ContainerDiskGb,
            VolumeDiskGb = request.VolumeDiskGb,
            VolumeMountPath = request.VolumeMountPath.Trim(),
            GpuCount = request.GpuCount,
            EnvironmentVariablesJson = PodAccess.SerializeEnvironmentVariables(request.EnvironmentVariables),
            PortsJson = PodAccess.SerializePorts(request.Ports),
            EnablePublicIp = request.EnablePublicIp,
        };

        pod.Configuration = configuration;

        var createOptions = new PodCreateOptions
        {
            Name = pod.Name,
            GpuId = pod.GpuId,
            GpuType = pod.GpuType,
            Region = pod.Region,
            TemplateId = pod.TemplateId,
            TemplateName = configuration.TemplateName,
            ImageName = pod.ImageName,
            ContainerDiskGb = request.ContainerDiskGb,
            VolumeDiskGb = request.VolumeDiskGb,
            VolumeMountPath = request.VolumeMountPath,
            GpuCount = request.GpuCount,
            EnvironmentVariables = request.EnvironmentVariables,
            Ports = request.Ports,
            EnablePublicIp = request.EnablePublicIp,
        };

        string? statusMessage = "Pod creation requested.";

        try
        {
            var providerInfo = await podService.CreatePodAsync(provider, createOptions, cancellationToken);
            podService.ApplyProviderInfo(pod, providerInfo, now);
            pod.UpdatedAt = now;
            pod.UpdatedBy = userId.ToString();
            statusMessage = providerInfo.StatusMessage;
        }
        catch (Exception ex)
        {
            pod.Status = PodStatus.Failed;
            pod.UpdatedAt = now;
            pod.UpdatedBy = userId.ToString();
            statusMessage = ex.Message;

            await PersistPodAsync(pod, now, statusMessage, cancellationToken);
            await podNotificationService.NotifyPodStatusChangedAsync(
                organizationId,
                pod.Id,
                pod.Status.ToString(),
                cancellationToken);
            throw;
        }

        await PersistPodAsync(pod, now, statusMessage, cancellationToken);

        await podLifecycleService.GetOrCreateIdlePolicyAsync(pod.Id, cancellationToken);
        pod.LastActivityAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await podNotificationService.NotifyPodStatusChangedAsync(
            organizationId,
            pod.Id,
            pod.Status.ToString(),
            cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(GpuPod),
            pod.Id.ToString(),
            $"Pod '{pod.Name}' created",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        pod.Provider = provider;
        return PodMapper.ToResponse(pod);
    }

    private async Task PersistPodAsync(
        GpuPod pod,
        DateTime recordedAt,
        string? message,
        CancellationToken cancellationToken)
    {
        await dbContext.AddGpuPodAsync(pod, cancellationToken);
        await dbContext.AddPodStatusHistoryAsync(
            new PodStatusHistory
            {
                GpuPodId = pod.Id,
                Status = pod.Status,
                RecordedAt = recordedAt,
                Message = message,
            },
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
