using System.Text.Json;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Deployments;
using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Deployments.Cloud;
using PodPilot.Infrastructure.Ollama;

namespace PodPilot.Infrastructure.Deployments;

/// <summary>
/// Orchestrates one-click AI pod deployments.
/// </summary>
public sealed class DeploymentService : IDeploymentService
{
    private static readonly DeploymentStatus[] ActiveWorkerStatuses =
    [
        DeploymentStatus.Pending,
        DeploymentStatus.Provisioning,
        DeploymentStatus.Starting,
        DeploymentStatus.InstallingRuntime,
        DeploymentStatus.Configuring,
        DeploymentStatus.HealthCheck,
    ];

    private readonly IApplicationDbContext db;
    private readonly IDeploymentCatalogService catalogService;
    private readonly IRuntimeProviderFactory runtimeProviderFactory;
    private readonly DeploymentCloudAdapterFactory cloudAdapterFactory;
    private readonly IProviderService providerService;
    private readonly IPodService podService;
    private readonly IPodLifecycleService podLifecycleService;
    private readonly IQuotaService quotaService;
    private readonly IAuditService auditService;
    private readonly IHttpContextService httpContextService;
    private readonly IDateTimeService dateTimeService;
    private readonly IDeploymentNotificationService notificationService;
    private readonly ILogger<DeploymentService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentService"/> class.
    /// </summary>
    public DeploymentService(
        IApplicationDbContext db,
        IDeploymentCatalogService catalogService,
        IRuntimeProviderFactory runtimeProviderFactory,
        DeploymentCloudAdapterFactory cloudAdapterFactory,
        IProviderService providerService,
        IPodService podService,
        IPodLifecycleService podLifecycleService,
        IQuotaService quotaService,
        IAuditService auditService,
        IHttpContextService httpContextService,
        IDateTimeService dateTimeService,
        IDeploymentNotificationService notificationService,
        ILogger<DeploymentService> logger)
    {
        this.db = db;
        this.catalogService = catalogService;
        this.runtimeProviderFactory = runtimeProviderFactory;
        this.cloudAdapterFactory = cloudAdapterFactory;
        this.providerService = providerService;
        this.podService = podService;
        this.podLifecycleService = podLifecycleService;
        this.quotaService = quotaService;
        this.auditService = auditService;
        this.httpContextService = httpContextService;
        this.dateTimeService = dateTimeService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    /// <summary>
    /// Worker helper: lists deployment ids in active pipeline statuses.
    /// </summary>
    public static DeploymentStatus[] GetActiveWorkerStatuses() => ActiveWorkerStatuses;

    /// <inheritdoc />
    public async Task<DeploymentDetail> CreateAsync(
        CreateDeploymentOptions options,
        CancellationToken cancellationToken = default)
    {
        await catalogService.EnsureSeededAsync(cancellationToken);

        var provider = await db.ComputeProviders
            .Include(p => p.Credential)
            .Include(p => p.Gpus)
            .FirstOrDefaultAsync(
                p => p.Id == options.ProviderId && p.OrganizationId == options.OrganizationId,
                cancellationToken);

        if (provider is null)
        {
            throw new NotFoundException("Provider", options.ProviderId);
        }

        if (!provider.IsEnabled || !provider.IsValidated)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(options.ProviderId),
                    "Provider must be enabled and validated before creating deployments."),
            ]);
        }

        if (provider.Credential is null)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(options.ProviderId), "Provider credentials are missing."),
            ]);
        }

        IDeploymentCloudAdapter adapter;
        try
        {
            adapter = cloudAdapterFactory.GetAdapterForProviderType(provider.ProviderType);
        }
        catch (InvalidOperationException ex)
        {
            throw new ForbiddenException(ex.Message);
        }

        var validation = await providerService.ValidateProviderAsync(provider, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(options.ProviderId),
                    validation.ErrorMessage ?? "Provider credential validation failed."),
            ]);
        }

        var gpu = await db.GpuCatalogEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(
                g => g.Code == options.GpuCode && g.IsActive,
                cancellationToken)
            ?? throw new ValidationException(
            [
                new ValidationFailure(nameof(options.GpuCode), $"Unknown GPU code '{options.GpuCode}'."),
            ]);

        var providerGpuId = ResolveProviderGpuId(options.ProviderGpuId, provider, gpu);

        DeploymentTemplate? template = null;
        if (!string.IsNullOrWhiteSpace(options.TemplateCode))
        {
            template = await db.DeploymentTemplates
                .FirstOrDefaultAsync(t => t.Code == options.TemplateCode.Trim(), cancellationToken);
            if (template is null)
            {
                throw new ValidationException(
                [
                    new ValidationFailure(nameof(options.TemplateCode), $"Unknown template '{options.TemplateCode}'."),
                ]);
            }
        }

        var catalogModels = await db.ModelCatalogEntries
            .AsNoTracking()
            .Where(m => m.IsActive)
            .ToListAsync(cancellationToken);

        var resolvedModels = new List<(string Reference, Guid? CatalogId, int RequiredVram)>();
        foreach (var modelKey in options.Models.Where(m => !string.IsNullOrWhiteSpace(m)))
        {
            var trimmed = modelKey.Trim();
            var entry = catalogModels.FirstOrDefault(m =>
                m.Code.Equals(trimmed, StringComparison.OrdinalIgnoreCase)
                || m.ModelReference.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

            if (entry is null)
            {
                resolvedModels.Add((trimmed, null, 0));
            }
            else
            {
                resolvedModels.Add((entry.ModelReference, entry.Id, entry.RequiredVramGb));
            }
        }

        var maxRequiredVram = resolvedModels.Count == 0 ? 0 : resolvedModels.Max(m => m.RequiredVram);
        if (maxRequiredVram > 0 && gpu.VramGb < maxRequiredVram)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(options.GpuCode),
                    $"GPU {gpu.Code} has {gpu.VramGb}GB VRAM but selected models require {maxRequiredVram}GB."),
            ]);
        }

        var runtimeProvider = runtimeProviderFactory.GetProvider(options.Runtime);
        await runtimeProvider.ValidateAsync(
            new RuntimeValidationContext
            {
                Runtime = options.Runtime,
                CudaVersion = "12.4",
                GpuVramGb = gpu.VramGb,
                CudaCapability = gpu.CudaCapability,
                RequiredVramGb = maxRequiredVram,
            },
            cancellationToken);

        await quotaService.EnsureCanCreatePodAsync(options.OrganizationId, cancellationToken);

        var now = dateTimeService.UtcNow;
        var image = template?.ContainerImage ?? runtimeProvider.GetDefaultImage("12.4");
        var deployment = new AiDeployment
        {
            Id = Guid.NewGuid(),
            OrganizationId = options.OrganizationId,
            Name = options.Name.Trim(),
            Status = DeploymentStatus.Pending,
            ProviderId = provider.Id,
            CloudProvider = adapter.Kind,
            Region = options.Region.Trim(),
            GpuCode = gpu.Code,
            ProviderGpuId = providerGpuId,
            Runtime = options.Runtime,
            CudaVersion = "12.4",
            TemplateId = template?.Id,
            ImageName = image,
            ProgressPercent = 0,
            StatusMessage = "Deployment Started",
            EstimatedHourlyCostUsd = gpu.EstimatedHourlyCostUsd,
            EnvironmentVariablesJson = options.EnvironmentVariables is null || options.EnvironmentVariables.Count == 0
                ? null
                : JsonSerializer.Serialize(options.EnvironmentVariables),
            CreatedAt = now,
            CreatedBy = options.UserId.ToString(),
        };

        for (var i = 0; i < resolvedModels.Count; i++)
        {
            var (reference, catalogId, _) = resolvedModels[i];
            deployment.Models.Add(new DeploymentModel
            {
                Id = Guid.NewGuid(),
                ModelCatalogId = catalogId,
                ModelReference = reference,
                DownloadStatus = DeploymentStatus.Pending,
                ProgressPercent = 0,
                IsPrimary = i == 0,
            });
        }

        deployment.Health = new DeploymentHealth
        {
            Id = Guid.NewGuid(),
            State = DeploymentHealthState.Unknown,
            CreatedAt = now,
        };

        await db.AddAiDeploymentAsync(deployment, cancellationToken);
        await LogAsync(deployment, DeploymentLogLevel.Info, "Create", "Deployment Started", now, cancellationToken);
        await TransitionStatusAsync(deployment, null, DeploymentStatus.Pending, "Queued", now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Created,
            nameof(AiDeployment),
            deployment.Id.ToString(),
            $"Deployment '{deployment.Name}' created",
            options.UserId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);

        await notificationService.NotifyStartedAsync(
            options.OrganizationId,
            deployment.Id,
            cancellationToken);

        return await GetAsync(options.OrganizationId, deployment.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeploymentSummary>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.AiDeployments
            .AsNoTracking()
            .Include(d => d.Health)
            .Where(d => d.OrganizationId == organizationId
                        && d.Status != DeploymentStatus.Cancelled
                        && d.Status != DeploymentStatus.Deleting)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(MapToSummary).ToList();
    }

    /// <inheritdoc />
    public async Task<DeploymentDetail> GetAsync(
        Guid organizationId,
        Guid deploymentId,
        CancellationToken cancellationToken = default)
    {
        var deployment = await db.AiDeployments
            .AsNoTracking()
            .Include(d => d.Models)
            .Include(d => d.Health)
            .FirstOrDefaultAsync(
                d => d.Id == deploymentId && d.OrganizationId == organizationId,
                cancellationToken);

        if (deployment is null)
        {
            throw new NotFoundException("Deployment", deploymentId);
        }

        var logs = await db.DeploymentLogs
            .AsNoTracking()
            .Where(l => l.DeploymentId == deploymentId)
            .OrderByDescending(l => l.TimestampUtc)
            .Take(50)
            .ToListAsync(cancellationToken);
        deployment.Logs = logs;

        return MapToDetail(deployment);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        Guid organizationId,
        Guid deploymentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var deployment = await LoadTrackedAsync(organizationId, deploymentId, cancellationToken);
        var now = dateTimeService.UtcNow;
        deployment.CancellationRequested = true;
        deployment.UpdatedAt = now;
        deployment.UpdatedBy = userId.ToString();

        if (deployment.Status is DeploymentStatus.Pending or DeploymentStatus.Failed or DeploymentStatus.Ready)
        {
            await TransitionStatusAsync(
                deployment,
                deployment.Status,
                DeploymentStatus.Deleting,
                "Deletion requested",
                now,
                cancellationToken);
        }

        await LogAsync(deployment, DeploymentLogLevel.Info, "Delete", "Cancellation requested", now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            AuditAction.Deleted,
            nameof(AiDeployment),
            deployment.Id.ToString(),
            $"Deployment '{deployment.Name}' deletion requested",
            userId,
            httpContextService.IpAddress,
            httpContextService.CorrelationId,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DeploymentDetail> RestartAsync(
        Guid organizationId,
        Guid deploymentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var deployment = await LoadTrackedAsync(organizationId, deploymentId, cancellationToken);
        if (deployment.Status is not(DeploymentStatus.Failed or DeploymentStatus.Ready or DeploymentStatus.Cancelled))
        {
            throw new ValidationException("Only failed, ready, or cancelled deployments can be restarted.");
        }

        var now = dateTimeService.UtcNow;
        deployment.CancellationRequested = false;
        deployment.ErrorMessage = null;
        deployment.RetryCount = 0;
        deployment.ProgressPercent = 0;
        deployment.ReadyAt = null;
        deployment.UpdatedAt = now;
        deployment.UpdatedBy = userId.ToString();

        var target = deployment.GpuPodId.HasValue
            ? DeploymentStatus.Starting
            : DeploymentStatus.Pending;

        await TransitionStatusAsync(deployment, deployment.Status, target, "Restart requested", now, cancellationToken);
        await LogAsync(deployment, DeploymentLogLevel.Info, "Restart", "Deployment restart queued", now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(organizationId, deploymentId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DeploymentHealthInfo> RunHealthCheckAsync(
        Guid organizationId,
        Guid deploymentId,
        CancellationToken cancellationToken = default)
    {
        var deployment = await db.AiDeployments
            .Include(d => d.Health)
            .Include(d => d.Models)
            .Include(d => d.GpuPod)!.ThenInclude(p => p!.Endpoints)
            .Include(d => d.GpuPod)!.ThenInclude(p => p!.Provider)!.ThenInclude(pr => pr!.Credential)
            .FirstOrDefaultAsync(
                d => d.Id == deploymentId && d.OrganizationId == organizationId,
                cancellationToken);

        if (deployment is null)
        {
            throw new NotFoundException("Deployment", deploymentId);
        }

        await PerformHealthCheckAsync(deployment, persistReadyTransition: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return MapHealth(deployment.Health!);
    }

    /// <inheritdoc />
    public async Task<DeploymentDashboardInfo> GetDashboardAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var deployments = await db.AiDeployments
            .AsNoTracking()
            .Include(d => d.Health)
            .Where(d => d.OrganizationId == organizationId
                        && d.Status != DeploymentStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var recent = deployments
            .OrderByDescending(d => d.CreatedAt)
            .Take(10)
            .Select(MapToSummary)
            .ToList();

        var runningHourly = deployments
            .Where(d => d.Status == DeploymentStatus.Ready)
            .Sum(d => d.EstimatedHourlyCostUsd);

        return new DeploymentDashboardInfo
        {
            RunningDeployments = deployments.Count(d => d.Status == DeploymentStatus.Ready),
            DownloadingModels = deployments.Count(d => d.Status == DeploymentStatus.DownloadingModels),
            HealthyDeployments = deployments.Count(d =>
                d.Status == DeploymentStatus.Ready
                && d.Health?.State == DeploymentHealthState.Healthy),
            FailedDeployments = deployments.Count(d => d.Status == DeploymentStatus.Failed),
            EstimatedMonthlyCostUsd = runningHourly * 24m * 30m,
            Recent = recent,
        };
    }

    /// <inheritdoc />
    public async Task ProcessPendingStepAsync(Guid deploymentId, CancellationToken cancellationToken = default)
    {
        var deployment = await db.AiDeployments
            .Include(d => d.Models)
            .Include(d => d.Health)
            .Include(d => d.Template)
            .Include(d => d.Provider).ThenInclude(p => p.Credential)
            .Include(d => d.Provider).ThenInclude(p => p.Gpus)
            .Include(d => d.GpuPod)!.ThenInclude(p => p!.Endpoints)
            .Include(d => d.GpuPod)!.ThenInclude(p => p!.Configuration)
            .FirstOrDefaultAsync(d => d.Id == deploymentId, cancellationToken);

        if (deployment is null)
        {
            return;
        }

        if (deployment.CancellationRequested
            && deployment.Status is not(DeploymentStatus.Deleting or DeploymentStatus.Cancelled))
        {
            var nowCancel = dateTimeService.UtcNow;
            await TransitionStatusAsync(
                deployment,
                deployment.Status,
                DeploymentStatus.Deleting,
                "Cancellation requested",
                nowCancel,
                cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        try
        {
            switch (deployment.Status)
            {
                case DeploymentStatus.Pending:
                    await TransitionStatusAsync(
                        deployment,
                        DeploymentStatus.Pending,
                        DeploymentStatus.Provisioning,
                        "Provisioning GPU pod",
                        dateTimeService.UtcNow,
                        cancellationToken);
                    deployment.ProgressPercent = 10;
                    await db.SaveChangesAsync(cancellationToken);
                    await ProcessProvisioningAsync(deployment, cancellationToken);
                    break;
                case DeploymentStatus.Provisioning:
                    await ProcessProvisioningAsync(deployment, cancellationToken);
                    break;
                case DeploymentStatus.Starting:
                    await ProcessStartingAsync(deployment, cancellationToken);
                    break;
                case DeploymentStatus.InstallingRuntime:
                    await ProcessInstallingRuntimeAsync(deployment, cancellationToken);
                    break;
                case DeploymentStatus.DownloadingModels:
                    await ProcessDownloadingModelsAsync(deployment, cancellationToken);
                    break;
                case DeploymentStatus.Configuring:
                    await ProcessConfiguringAsync(deployment, cancellationToken);
                    break;
                case DeploymentStatus.HealthCheck:
                    await ProcessHealthCheckStepAsync(deployment, cancellationToken);
                    break;
                case DeploymentStatus.Deleting:
                    await ProcessDeletingAsync(deployment, cancellationToken);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Deployment step failed for {DeploymentId}", deploymentId);
            await FailAsync(deployment, ex.Message, cancellationToken);
        }
    }

    private async Task ProcessProvisioningAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        if (deployment.GpuPodId.HasValue)
        {
            await TransitionStatusAsync(
                deployment,
                deployment.Status,
                DeploymentStatus.Starting,
                "Pod already provisioned",
                dateTimeService.UtcNow,
                cancellationToken);
            deployment.ProgressPercent = 25;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var now = dateTimeService.UtcNow;
        var runtime = runtimeProviderFactory.GetProvider(deployment.Runtime);
        var image = deployment.ImageName ?? runtime.GetDefaultImage(deployment.CudaVersion);
        var port = $"{runtime.DefaultPort}/http";
        var gpuType = await ResolveGpuTypeAsync(deployment.GpuCode, cancellationToken);
        var env = DeserializeEnv(deployment.EnvironmentVariablesJson);

        var pod = new GpuPod
        {
            Id = Guid.NewGuid(),
            OrganizationId = deployment.OrganizationId,
            ProviderId = deployment.ProviderId,
            Name = $"{deployment.Name}-pod",
            Description = $"One-click deployment {deployment.Name}",
            Status = PodStatus.BuildingPending,
            GpuType = gpuType,
            GpuId = deployment.ProviderGpuId,
            Region = deployment.Region,
            ImageName = image,
            ContainerDisk = 50,
            VolumeDisk = 100,
            IsPublic = true,
            CreatedAt = now,
            CreatedBy = "deployment-worker",
        };

        var configuration = new PodConfiguration
        {
            ImageName = image,
            ContainerDiskGb = 50,
            VolumeDiskGb = 100,
            VolumeMountPath = "/workspace",
            GpuCount = 1,
            EnvironmentVariablesJson = env.Count == 0 ? null : JsonSerializer.Serialize(env),
            PortsJson = JsonSerializer.Serialize(new[] { port }),
            EnablePublicIp = true,
        };
        pod.Configuration = configuration;

        var createOptions = new PodCreateOptions
        {
            Name = pod.Name,
            GpuId = pod.GpuId,
            GpuType = pod.GpuType,
            Region = pod.Region,
            ImageName = image,
            ContainerDiskGb = 50,
            VolumeDiskGb = 100,
            VolumeMountPath = "/workspace",
            GpuCount = 1,
            EnvironmentVariables = env,
            Ports = [port],
            EnablePublicIp = true,
        };

        try
        {
            var providerInfo = await podService.CreatePodAsync(deployment.Provider, createOptions, cancellationToken);
            podService.ApplyProviderInfo(pod, providerInfo, now);
        }
        catch (Exception ex)
        {
            pod.Status = PodStatus.Failed;
            await db.AddGpuPodAsync(pod, cancellationToken);
            await db.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = now,
                    Message = ex.Message,
                },
                cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }

        await db.AddGpuPodAsync(pod, cancellationToken);
        await db.AddPodStatusHistoryAsync(
            new PodStatusHistory
            {
                GpuPodId = pod.Id,
                Status = pod.Status,
                RecordedAt = now,
                Message = "Created by one-click deployment",
            },
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await podLifecycleService.GetOrCreateIdlePolicyAsync(pod.Id, cancellationToken);

        deployment.GpuPodId = pod.Id;
        deployment.GpuPod = pod;
        deployment.ImageName = image;
        deployment.ProgressPercent = 25;
        await LogAsync(deployment, DeploymentLogLevel.Info, "Provisioning", $"Pod {pod.Id} created", now, cancellationToken);
        await TransitionStatusAsync(
            deployment,
            DeploymentStatus.Provisioning,
            DeploymentStatus.Starting,
            "Waiting for pod to start",
            now,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        await notificationService.NotifyProgressAsync(
            deployment.OrganizationId,
            deployment.Id,
            deployment.Status,
            deployment.ProgressPercent,
            deployment.StatusMessage,
            cancellationToken);
    }

    private async Task ProcessStartingAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        if (!deployment.GpuPodId.HasValue)
        {
            throw new InvalidOperationException("Deployment has no linked pod.");
        }

        var pod = deployment.GpuPod
            ?? await db.GpuPods
                .Include(p => p.Endpoints)
                .Include(p => p.Provider).ThenInclude(pr => pr.Credential)
                .FirstAsync(p => p.Id == deployment.GpuPodId.Value, cancellationToken);

        if (pod.Provider is null)
        {
            pod.Provider = deployment.Provider;
        }

        await podService.SyncPodStatusAsync(pod, cancellationToken);

        if (pod.Status == PodStatus.Running)
        {
            var now = dateTimeService.UtcNow;
            deployment.ProgressPercent = 40;
            await TransitionStatusAsync(
                deployment,
                DeploymentStatus.Starting,
                DeploymentStatus.InstallingRuntime,
                "Installing / verifying runtime",
                now,
                cancellationToken);
            await LogAsync(deployment, DeploymentLogLevel.Info, "Starting", "Pod is running", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await notificationService.NotifyProgressAsync(
                deployment.OrganizationId,
                deployment.Id,
                deployment.Status,
                deployment.ProgressPercent,
                deployment.StatusMessage,
                cancellationToken);
            return;
        }

        if (pod.Status is PodStatus.Failed or PodStatus.Deleted)
        {
            throw new InvalidOperationException($"Pod entered terminal status {pod.Status}.");
        }

        deployment.StatusMessage = $"Waiting for pod (status={pod.Status})";
        deployment.UpdatedAt = dateTimeService.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessInstallingRuntimeAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        var pod = await RequirePodWithEndpointsAsync(deployment, cancellationToken);
        var runtime = runtimeProviderFactory.GetProvider(deployment.Runtime);
        var baseUrl = ResolveRuntimeBaseUrl(pod, runtime.DefaultPort);

        await runtime.EnsureInstalledAsync(
            new RuntimeExecutionContext
            {
                OrganizationId = deployment.OrganizationId,
                DeploymentId = deployment.Id,
                GpuPodId = pod.Id,
                BaseUrl = baseUrl,
                CudaVersion = deployment.CudaVersion,
            },
            cancellationToken);

        var now = dateTimeService.UtcNow;
        deployment.ProgressPercent = 55;
        await TransitionStatusAsync(
            deployment,
            DeploymentStatus.InstallingRuntime,
            DeploymentStatus.DownloadingModels,
            "Downloading models",
            now,
            cancellationToken);
        await LogAsync(deployment, DeploymentLogLevel.Info, "Runtime", "Runtime is reachable", now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyProgressAsync(
            deployment.OrganizationId,
            deployment.Id,
            deployment.Status,
            deployment.ProgressPercent,
            deployment.StatusMessage,
            cancellationToken);
    }

    private async Task ProcessDownloadingModelsAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        var pod = await RequirePodWithEndpointsAsync(deployment, cancellationToken);
        var runtime = runtimeProviderFactory.GetProvider(deployment.Runtime);
        var baseUrl = ResolveRuntimeBaseUrl(pod, runtime.DefaultPort);
        var context = new RuntimeExecutionContext
        {
            OrganizationId = deployment.OrganizationId,
            DeploymentId = deployment.Id,
            GpuPodId = pod.Id,
            BaseUrl = baseUrl,
            CudaVersion = deployment.CudaVersion,
        };

        var pending = deployment.Models
            .Where(m => m.DownloadStatus != DeploymentStatus.Ready)
            .OrderByDescending(m => m.IsPrimary)
            .ToList();

        foreach (var model in pending)
        {
            if (await runtime.IsModelAvailableAsync(context, model.ModelReference, cancellationToken))
            {
                model.DownloadStatus = DeploymentStatus.Ready;
                model.ProgressPercent = 100;
                continue;
            }

            model.DownloadStatus = DeploymentStatus.DownloadingModels;
            var progress = new Progress<int>(percent =>
            {
                model.ProgressPercent = percent;
            });

            try
            {
                await runtime.PullModelAsync(context, model.ModelReference, progress, cancellationToken);
                model.DownloadStatus = DeploymentStatus.Ready;
                model.ProgressPercent = 100;
                model.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                model.DownloadStatus = DeploymentStatus.Failed;
                model.ErrorMessage = ex.Message;
                throw;
            }

            await notificationService.NotifyModelProgressAsync(
                deployment.OrganizationId,
                deployment.Id,
                model.ModelReference,
                model.ProgressPercent,
                cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (deployment.Models.Any(m => m.DownloadStatus != DeploymentStatus.Ready))
        {
            throw new InvalidOperationException("One or more models failed to download.");
        }

        var now = dateTimeService.UtcNow;
        deployment.ProgressPercent = 80;
        await TransitionStatusAsync(
            deployment,
            DeploymentStatus.DownloadingModels,
            DeploymentStatus.Configuring,
            "Configuring gateway",
            now,
            cancellationToken);
        await LogAsync(deployment, DeploymentLogLevel.Info, "Models", "All models available", now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessConfiguringAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        if (!deployment.GpuPodId.HasValue)
        {
            throw new InvalidOperationException("Deployment has no linked pod.");
        }

        var primary = deployment.Models.FirstOrDefault(m => m.IsPrimary)
            ?? deployment.Models.FirstOrDefault()
            ?? throw new InvalidOperationException("Deployment has no models.");

        var now = dateTimeService.UtcNow;
        if (!deployment.GatewayRouteId.HasValue)
        {
            var existing = await db.GatewayRoutes.FirstOrDefaultAsync(
                r => r.OrganizationId == deployment.OrganizationId
                     && r.ModelName == primary.ModelReference,
                cancellationToken);

            if (existing is null)
            {
                var hasDefault = await db.GatewayRoutes.AnyAsync(
                    r => r.OrganizationId == deployment.OrganizationId && r.IsDefault,
                    cancellationToken);

                var route = new GatewayRoute
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = deployment.OrganizationId,
                    GpuPodId = deployment.GpuPodId.Value,
                    ModelName = primary.ModelReference,
                    IsDefault = !hasDefault,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                await db.AddGatewayRouteAsync(route, cancellationToken);
                deployment.GatewayRouteId = route.Id;
            }
            else
            {
                existing.GpuPodId = deployment.GpuPodId.Value;
                existing.UpdatedAt = now;
                deployment.GatewayRouteId = existing.Id;
            }
        }

        deployment.ProgressPercent = 90;
        await TransitionStatusAsync(
            deployment,
            DeploymentStatus.Configuring,
            DeploymentStatus.HealthCheck,
            "Running health checks",
            now,
            cancellationToken);
        await LogAsync(deployment, DeploymentLogLevel.Info, "Gateway", "Gateway route configured", now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessHealthCheckStepAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        await PerformHealthCheckAsync(deployment, persistReadyTransition: true, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task PerformHealthCheckAsync(
        AiDeployment deployment,
        bool persistReadyTransition,
        CancellationToken cancellationToken)
    {
        deployment.Health ??= new DeploymentHealth
        {
            Id = Guid.NewGuid(),
            DeploymentId = deployment.Id,
            State = DeploymentHealthState.Unknown,
            CreatedAt = dateTimeService.UtcNow,
        };

        if (!deployment.GpuPodId.HasValue)
        {
            deployment.Health.State = DeploymentHealthState.Unhealthy;
            deployment.Health.LastCheckedAt = dateTimeService.UtcNow;
            return;
        }

        var pod = await RequirePodWithEndpointsAsync(deployment, cancellationToken);
        var runtime = runtimeProviderFactory.GetProvider(deployment.Runtime);
        var baseUrl = ResolveRuntimeBaseUrl(pod, runtime.DefaultPort);
        var runtimeHealth = await runtime.CheckHealthAsync(
            new RuntimeExecutionContext
            {
                OrganizationId = deployment.OrganizationId,
                DeploymentId = deployment.Id,
                GpuPodId = pod.Id,
                BaseUrl = baseUrl,
                CudaVersion = deployment.CudaVersion,
            },
            cancellationToken);

        var modelsOk = true;
        foreach (var model in deployment.Models)
        {
            if (!await runtime.IsModelAvailableAsync(
                    new RuntimeExecutionContext
                    {
                        OrganizationId = deployment.OrganizationId,
                        DeploymentId = deployment.Id,
                        GpuPodId = pod.Id,
                        BaseUrl = baseUrl,
                        CudaVersion = deployment.CudaVersion,
                    },
                    model.ModelReference,
                    cancellationToken))
            {
                modelsOk = false;
                break;
            }
        }

        var gatewayOk = deployment.GatewayRouteId.HasValue
            && await db.GatewayRoutes.AnyAsync(r => r.Id == deployment.GatewayRouteId.Value, cancellationToken);

        deployment.Health.GpuAvailable = runtimeHealth.GpuAvailable;
        deployment.Health.CudaAvailable = runtimeHealth.CudaAvailable;
        deployment.Health.RuntimeRunning = runtimeHealth.RuntimeRunning;
        deployment.Health.ModelAvailable = modelsOk;
        deployment.Health.GatewayReachable = gatewayOk;
        deployment.Health.StreamingWorks = runtimeHealth.StreamingWorks;
        deployment.Health.LastCheckedAt = dateTimeService.UtcNow;
        deployment.Health.UpdatedAt = dateTimeService.UtcNow;
        deployment.Health.DetailsJson = JsonSerializer.Serialize(new
        {
            runtimeHealth.Message,
        });

        var healthy = runtimeHealth.RuntimeRunning && modelsOk && gatewayOk;
        deployment.Health.State = healthy
            ? DeploymentHealthState.Healthy
            : runtimeHealth.RuntimeRunning
                ? DeploymentHealthState.Degraded
                : DeploymentHealthState.Unhealthy;

        await notificationService.NotifyHealthAsync(
            deployment.OrganizationId,
            deployment.Id,
            deployment.Health.State,
            cancellationToken);

        if (!persistReadyTransition)
        {
            return;
        }

        var now = dateTimeService.UtcNow;
        if (healthy)
        {
            deployment.ProgressPercent = 100;
            deployment.ReadyAt = now;
            deployment.ErrorMessage = null;
            await TransitionStatusAsync(
                deployment,
                DeploymentStatus.HealthCheck,
                DeploymentStatus.Ready,
                "Deployment ready",
                now,
                cancellationToken);
            await LogAsync(deployment, DeploymentLogLevel.Info, "Health", "Healthy", now, cancellationToken);
            await notificationService.NotifyReadyAsync(
                deployment.OrganizationId,
                deployment.Id,
                cancellationToken);
        }
        else
        {
            throw new InvalidOperationException(runtimeHealth.Message ?? "Deployment health check failed.");
        }
    }

    private async Task ProcessDeletingAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        var now = dateTimeService.UtcNow;
        if (deployment.GpuPodId.HasValue)
        {
            var pod = await db.GpuPods
                .Include(p => p.Provider).ThenInclude(pr => pr.Credential)
                .FirstOrDefaultAsync(p => p.Id == deployment.GpuPodId.Value, cancellationToken);

            if (pod is not null
                && pod.Status != PodStatus.Deleted
                && !string.IsNullOrWhiteSpace(pod.ProviderPodId)
                && pod.Provider is not null)
            {
                try
                {
                    await podService.DeletePodAsync(pod.Provider, pod.ProviderPodId, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to delete pod {PodId} for deployment {DeploymentId}", pod.Id, deployment.Id);
                }

                pod.Status = PodStatus.Deleted;
                pod.UpdatedAt = now;
            }
        }

        await TransitionStatusAsync(
            deployment,
            DeploymentStatus.Deleting,
            DeploymentStatus.Cancelled,
            "Deployment cancelled",
            now,
            cancellationToken);
        deployment.ProgressPercent = 0;
        await LogAsync(deployment, DeploymentLogLevel.Info, "Delete", "Deployment cancelled", now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task FailAsync(AiDeployment deployment, string error, CancellationToken cancellationToken)
    {
        var now = dateTimeService.UtcNow;
        deployment.ErrorMessage = error.Length > 2000 ? error[..2000] : error;
        deployment.StatusMessage = "Deployment failed";
        await TransitionStatusAsync(
            deployment,
            deployment.Status,
            DeploymentStatus.Failed,
            error,
            now,
            cancellationToken);
        await LogAsync(deployment, DeploymentLogLevel.Error, "Failed", error, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyFailedAsync(
            deployment.OrganizationId,
            deployment.Id,
            error,
            cancellationToken);
    }

    private async Task<AiDeployment> LoadTrackedAsync(
        Guid organizationId,
        Guid deploymentId,
        CancellationToken cancellationToken)
    {
        var deployment = await db.AiDeployments
            .FirstOrDefaultAsync(
                d => d.Id == deploymentId && d.OrganizationId == organizationId,
                cancellationToken);

        if (deployment is null)
        {
            throw new NotFoundException("Deployment", deploymentId);
        }

        return deployment;
    }

    private async Task<GpuPod> RequirePodWithEndpointsAsync(AiDeployment deployment, CancellationToken cancellationToken)
    {
        if (!deployment.GpuPodId.HasValue)
        {
            throw new InvalidOperationException("Deployment has no linked pod.");
        }

        if (deployment.GpuPod?.Endpoints is { Count: > 0 })
        {
            return deployment.GpuPod;
        }

        return await db.GpuPods
            .Include(p => p.Endpoints)
            .Include(p => p.Provider).ThenInclude(pr => pr.Credential)
            .FirstAsync(p => p.Id == deployment.GpuPodId.Value, cancellationToken);
    }

    private async Task<GpuType> ResolveGpuTypeAsync(string gpuCode, CancellationToken cancellationToken)
    {
        var entry = await db.GpuCatalogEntries.AsNoTracking()
            .FirstOrDefaultAsync(g => g.Code == gpuCode, cancellationToken);
        return entry?.GpuType ?? GpuType.Custom;
    }

    private static string ResolveProviderGpuId(
        string? overrideId,
        ComputeProvider provider,
        GpuCatalogEntry gpu)
    {
        if (!string.IsNullOrWhiteSpace(overrideId))
        {
            return overrideId.Trim();
        }

        var match = provider.Gpus.FirstOrDefault(g => g.GpuType == gpu.GpuType)
            ?? provider.Gpus.FirstOrDefault(g =>
                g.Name.Contains(gpu.Code, StringComparison.OrdinalIgnoreCase)
                || g.GpuId.Contains(gpu.Code, StringComparison.OrdinalIgnoreCase));

        return match?.GpuId ?? gpu.Code;
    }

    private static string ResolveRuntimeBaseUrl(GpuPod pod, int port)
    {
        if (port == ApplicationConstants.OllamaPort)
        {
            return OllamaUrlHelper.GetOllamaBaseUrl(pod);
        }

        var endpoint = pod.Endpoints.FirstOrDefault(e =>
            e.Port == port && e.Protocol.Equals("http", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(endpoint?.Url))
        {
            return endpoint.Url.TrimEnd('/');
        }

        if (endpoint?.PublicPort is int publicPort && !string.IsNullOrWhiteSpace(pod.PublicIp))
        {
            return $"http://{pod.PublicIp}:{publicPort}";
        }

        if (!string.IsNullOrWhiteSpace(pod.PublicIp))
        {
            return $"http://{pod.PublicIp}:{port}";
        }

        throw new InvalidOperationException($"Pod '{pod.Name}' has no reachable endpoint on port {port}.");
    }

    private static IReadOnlyDictionary<string, string> DeserializeEnv(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? new Dictionary<string, string>();
    }

    private async Task LogAsync(
        AiDeployment deployment,
        DeploymentLogLevel level,
        string stage,
        string message,
        DateTime timestampUtc,
        CancellationToken cancellationToken)
    {
        await db.AddDeploymentLogAsync(
            new DeploymentLog
            {
                Id = Guid.NewGuid(),
                DeploymentId = deployment.Id,
                Level = level,
                Stage = stage,
                Message = message.Length > 4000 ? message[..4000] : message,
                TimestampUtc = timestampUtc,
            },
            cancellationToken);
    }

    private async Task TransitionStatusAsync(
        AiDeployment deployment,
        DeploymentStatus? from,
        DeploymentStatus to,
        string? message,
        DateTime timestampUtc,
        CancellationToken cancellationToken)
    {
        var previous = from ?? deployment.Status;
        deployment.Status = to;
        deployment.StatusMessage = message;
        deployment.UpdatedAt = timestampUtc;

        await db.AddDeploymentHistoryAsync(
            new DeploymentHistory
            {
                Id = Guid.NewGuid(),
                DeploymentId = deployment.Id,
                FromStatus = previous == to ? null : previous,
                ToStatus = to,
                Message = message,
                TimestampUtc = timestampUtc,
            },
            cancellationToken);

        if (to != DeploymentStatus.Pending || from is not null)
        {
            await notificationService.NotifyProgressAsync(
                deployment.OrganizationId,
                deployment.Id,
                to,
                deployment.ProgressPercent,
                message,
                cancellationToken);
        }
    }

    private static DeploymentSummary MapToSummary(AiDeployment d) =>
        new()
        {
            Id = d.Id,
            Name = d.Name,
            Status = d.Status,
            Runtime = d.Runtime,
            GpuCode = d.GpuCode,
            Region = d.Region,
            ProgressPercent = d.ProgressPercent,
            StatusMessage = d.StatusMessage,
            HealthState = d.Health?.State ?? DeploymentHealthState.Unknown,
            EstimatedHourlyCostUsd = d.EstimatedHourlyCostUsd,
            CreatedAt = d.CreatedAt,
            GpuPodId = d.GpuPodId,
        };

    private static DeploymentDetail MapToDetail(AiDeployment d) =>
        new()
        {
            Id = d.Id,
            Name = d.Name,
            Status = d.Status,
            Runtime = d.Runtime,
            GpuCode = d.GpuCode,
            Region = d.Region,
            ProgressPercent = d.ProgressPercent,
            StatusMessage = d.StatusMessage,
            HealthState = d.Health?.State ?? DeploymentHealthState.Unknown,
            EstimatedHourlyCostUsd = d.EstimatedHourlyCostUsd,
            CreatedAt = d.CreatedAt,
            GpuPodId = d.GpuPodId,
            ProviderId = d.ProviderId,
            CloudProvider = d.CloudProvider,
            CudaVersion = d.CudaVersion,
            ImageName = d.ImageName,
            ErrorMessage = d.ErrorMessage,
            GatewayRouteId = d.GatewayRouteId,
            ReadyAt = d.ReadyAt,
            Models = d.Models.Select(m => new DeploymentModelInfo
            {
                Id = m.Id,
                ModelReference = m.ModelReference,
                DownloadStatus = m.DownloadStatus,
                ProgressPercent = m.ProgressPercent,
                IsPrimary = m.IsPrimary,
                ErrorMessage = m.ErrorMessage,
            }).ToList(),
            Logs = d.Logs
                .OrderByDescending(l => l.TimestampUtc)
                .Select(l => new DeploymentLogInfo
                {
                    Id = l.Id,
                    Level = l.Level,
                    Stage = l.Stage,
                    Message = l.Message,
                    TimestampUtc = l.TimestampUtc,
                }).ToList(),
            Health = d.Health is null ? null : MapHealth(d.Health),
        };

    private static DeploymentHealthInfo MapHealth(DeploymentHealth h) =>
        new()
        {
            State = h.State,
            GpuAvailable = h.GpuAvailable,
            CudaAvailable = h.CudaAvailable,
            RuntimeRunning = h.RuntimeRunning,
            ModelAvailable = h.ModelAvailable,
            GatewayReachable = h.GatewayReachable,
            StreamingWorks = h.StreamingWorks,
            LastCheckedAt = h.LastCheckedAt,
        };
}
