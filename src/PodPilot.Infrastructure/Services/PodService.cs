using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Orchestrates pod operations with provider adapters and persistence.
/// </summary>
public sealed class PodService : IPodService
{
    private readonly IPodProviderFactory podProviderFactory;
    private readonly IProviderService providerService;
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PodService"/> class.
    /// </summary>
    public PodService(
        IPodProviderFactory podProviderFactory,
        IProviderService providerService,
        IApplicationDbContext dbContext)
    {
        this.podProviderFactory = podProviderFactory;
        this.providerService = providerService;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PodInfo> CreatePodAsync(
        ComputeProvider provider,
        PodCreateOptions options,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await providerService.GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await podProviderFactory.GetProvider(provider.ProviderType)
            .CreatePodAsync(apiKey, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PodOperationResult> DeletePodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await providerService.GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await podProviderFactory.GetProvider(provider.ProviderType)
            .DeletePodAsync(apiKey, providerPodId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PodOperationResult> StartPodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await providerService.GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await podProviderFactory.GetProvider(provider.ProviderType)
            .StartPodAsync(apiKey, providerPodId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PodOperationResult> StopPodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await providerService.GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await podProviderFactory.GetProvider(provider.ProviderType)
            .StopPodAsync(apiKey, providerPodId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PodOperationResult> RestartPodAsync(
        ComputeProvider provider,
        string providerPodId,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await providerService.GetDecryptedApiKeyAsync(provider, cancellationToken);
        return await podProviderFactory.GetProvider(provider.ProviderType)
            .RestartPodAsync(apiKey, providerPodId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ImportProviderPodsAsync(
        ComputeProvider provider,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (!provider.IsEnabled || !provider.IsValidated || provider.Credential is null)
        {
            return;
        }

        var apiKey = await providerService.GetDecryptedApiKeyAsync(provider, cancellationToken);
        var providerPods = await podProviderFactory.GetProvider(provider.ProviderType)
            .ListPodsAsync(apiKey, cancellationToken);

        if (providerPods.Count == 0)
        {
            return;
        }

        var trackedProviderPodIds = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId && p.ProviderId == provider.Id && p.ProviderPodId != null)
            .Select(p => p.ProviderPodId!)
            .ToListAsync(cancellationToken);

        var trackedIds = trackedProviderPodIds.ToHashSet(StringComparer.Ordinal);
        var existingNames = await dbContext.GpuPods
            .Where(p => p.OrganizationId == organizationId && p.Status != PodStatus.Deleted)
            .Select(p => p.Name)
            .ToListAsync(cancellationToken);

        var usedNames = existingNames.ToHashSet(StringComparer.Ordinal);
        var now = DateTime.UtcNow;
        var imported = false;

        foreach (var info in providerPods)
        {
            if (string.IsNullOrWhiteSpace(info.ProviderPodId)
                || info.Status == PodStatus.Deleted
                || trackedIds.Contains(info.ProviderPodId))
            {
                continue;
            }

            var pod = new GpuPod
            {
                OrganizationId = organizationId,
                ProviderId = provider.Id,
                Name = ResolveImportName(usedNames, info.Name, info.ProviderPodId),
                Status = info.Status,
                GpuType = info.GpuType,
                GpuId = string.IsNullOrWhiteSpace(info.GpuId) ? "unknown" : info.GpuId,
                Region = string.IsNullOrWhiteSpace(info.Region) ? "unknown" : info.Region,
                TemplateId = info.TemplateId,
                ImageName = string.IsNullOrWhiteSpace(info.ImageName) ? "unknown" : info.ImageName,
                ContainerDisk = info.ContainerDiskGb ?? 0,
                VolumeDisk = info.VolumeDiskGb ?? 0,
                IsPublic = !string.IsNullOrWhiteSpace(info.PublicIp) || info.Endpoints.Count > 0,
                CreatedAt = now,
            };

            ApplyProviderInfo(pod, info, now);

            if (info.Status == PodStatus.Stopped)
            {
                pod.LastStoppedAt = info.LastStoppedAt ?? now;
            }

            await dbContext.AddGpuPodAsync(pod, cancellationToken);
            await dbContext.AddPodStatusHistoryAsync(
                new PodStatusHistory
                {
                    GpuPodId = pod.Id,
                    Status = pod.Status,
                    RecordedAt = now,
                    Message = "Imported from provider.",
                },
                cancellationToken);

            trackedIds.Add(info.ProviderPodId);
            imported = true;
        }

        if (imported)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<PodInfo> SyncPodStatusAsync(
        GpuPod pod,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pod.ProviderPodId))
        {
            return new PodInfo
            {
                ProviderPodId = string.Empty,
                Name = pod.Name,
                Status = pod.Status,
            };
        }

        var provider = await dbContext.ComputeProviders
            .Include(p => p.Credential)
            .FirstAsync(p => p.Id == pod.ProviderId, cancellationToken);

        var apiKey = await providerService.GetDecryptedApiKeyAsync(provider, cancellationToken);
        var info = await podProviderFactory.GetProvider(provider.ProviderType)
            .SyncPodStatusAsync(apiKey, pod.ProviderPodId, cancellationToken);

        var syncedAt = DateTime.UtcNow;
        ApplyProviderScalars(pod, info, syncedAt);
        await ReplacePodEndpointsAsync(pod, info, cancellationToken);
        return info;
    }

    /// <inheritdoc />
    public void ApplyProviderStatus(GpuPod pod, PodInfo info, DateTime syncedAt)
    {
        pod.ProviderPodId = info.ProviderPodId;
        pod.Status = info.Status;
        pod.PublicIp = info.PublicIp;
        pod.Endpoint = info.Endpoint;
        pod.HourlyCost = info.HourlyCost;
        pod.LastStartedAt = info.LastStartedAt ?? pod.LastStartedAt;
        pod.LastStoppedAt = info.LastStoppedAt ?? pod.LastStoppedAt;
        pod.LastSyncedAt = syncedAt;
    }

    /// <inheritdoc />
    public void ApplyProviderInfo(GpuPod pod, PodInfo info, DateTime syncedAt)
    {
        ApplyProviderScalars(pod, info, syncedAt);

        foreach (var endpoint in info.Endpoints)
        {
            pod.Endpoints.Add(CreatePodEndpoint(pod.Id, endpoint));
        }
    }

    private async Task ReplacePodEndpointsAsync(
        GpuPod pod,
        PodInfo info,
        CancellationToken cancellationToken)
    {
        await dbContext.RemovePodEndpointsAsync(pod.Id, cancellationToken);

        foreach (var endpoint in pod.Endpoints.ToList())
        {
            pod.Endpoints.Remove(endpoint);
        }

        foreach (var endpoint in info.Endpoints)
        {
            await dbContext.AddPodEndpointAsync(CreatePodEndpoint(pod.Id, endpoint), cancellationToken);
        }
    }

    private static PodEndpoint CreatePodEndpoint(Guid podId, PodEndpointInfo endpoint) =>
        new()
        {
            GpuPodId = podId,
            Port = endpoint.Port,
            Protocol = endpoint.Protocol,
            PublicPort = endpoint.PublicPort,
            Url = endpoint.Url,
        };

    private static void ApplyProviderScalars(GpuPod pod, PodInfo info, DateTime syncedAt)
    {
        pod.ProviderPodId = info.ProviderPodId;
        pod.Status = info.Status;
        pod.PublicIp = info.PublicIp;
        pod.Endpoint = info.Endpoint;
        pod.HourlyCost = info.HourlyCost;
        pod.LastStartedAt = info.LastStartedAt ?? pod.LastStartedAt;
        pod.LastStoppedAt = info.LastStoppedAt ?? pod.LastStoppedAt;
        pod.LastSyncedAt = syncedAt;

        if (!string.IsNullOrWhiteSpace(info.GpuId))
        {
            pod.GpuId = info.GpuId;
        }

        if (!string.IsNullOrWhiteSpace(info.Region))
        {
            pod.Region = info.Region;
        }

        if (!string.IsNullOrWhiteSpace(info.ImageName))
        {
            pod.ImageName = info.ImageName;
        }

        if (info.ContainerDiskGb.HasValue)
        {
            pod.ContainerDisk = info.ContainerDiskGb.Value;
        }

        if (info.VolumeDiskGb.HasValue)
        {
            pod.VolumeDisk = info.VolumeDiskGb.Value;
        }

        if (info.TemplateId is not null)
        {
            pod.TemplateId = info.TemplateId;
        }
    }

    private static string ResolveImportName(HashSet<string> usedNames, string? providerName, string providerPodId)
    {
        var baseName = string.IsNullOrWhiteSpace(providerName)
            ? $"pod-{providerPodId[..Math.Min(8, providerPodId.Length)]}"
            : providerName.Trim();

        if (baseName.Length > ApplicationConstants.PodNameMaxLength)
        {
            baseName = baseName[..ApplicationConstants.PodNameMaxLength];
        }

        if (!usedNames.Contains(baseName))
        {
            usedNames.Add(baseName);
            return baseName;
        }

        var suffix = providerPodId.Length >= 6 ? providerPodId[..6] : providerPodId;
        var maxBaseLength = ApplicationConstants.PodNameMaxLength - suffix.Length - 1;
        if (maxBaseLength < 1)
        {
            maxBaseLength = ApplicationConstants.PodNameMaxLength - suffix.Length;
        }

        var truncatedBase = baseName.Length > maxBaseLength ? baseName[..maxBaseLength] : baseName;
        var candidate = $"{truncatedBase}-{suffix}";
        if (candidate.Length > ApplicationConstants.PodNameMaxLength)
        {
            candidate = candidate[..ApplicationConstants.PodNameMaxLength];
        }

        while (usedNames.Contains(candidate))
        {
            candidate = $"{truncatedBase}-{Guid.NewGuid():N}"[..ApplicationConstants.PodNameMaxLength];
        }

        usedNames.Add(candidate);
        return candidate;
    }
}
