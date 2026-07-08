using PodPilot.Contracts.Pods;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Pods;

/// <summary>
/// Maps pod entities to contract responses.
/// </summary>
internal static class PodMapper
{
    /// <summary>
    /// Maps a GPU pod entity to a response DTO.
    /// </summary>
    public static PodResponse ToResponse(GpuPod pod, bool includeHistory = false)
    {
        var history = includeHistory
            ? pod.StatusHistory
                .OrderByDescending(h => h.RecordedAt)
                .Take(20)
                .Select(h => new PodStatusHistoryResponse
                {
                    Status = FormatStatus(h.Status),
                    RecordedAt = h.RecordedAt,
                    Message = h.Message,
                })
                .ToList()
            : [];

        return new PodResponse
        {
            Id = pod.Id,
            OrganizationId = pod.OrganizationId,
            ProviderId = pod.ProviderId,
            ProviderName = pod.Provider?.DisplayName ?? string.Empty,
            ProviderType = pod.Provider?.ProviderType.ToString() ?? string.Empty,
            ProviderPodId = pod.ProviderPodId,
            Name = pod.Name,
            Description = pod.Description,
            Status = FormatStatus(pod.Status),
            GpuType = pod.GpuType.ToString(),
            GpuId = pod.GpuId,
            Region = pod.Region,
            TemplateId = pod.TemplateId,
            ImageName = pod.ImageName,
            ContainerDisk = pod.ContainerDisk,
            VolumeDisk = pod.VolumeDisk,
            PublicIp = pod.PublicIp,
            Endpoint = pod.Endpoint,
            IsPublic = pod.IsPublic,
            HourlyCost = pod.HourlyCost,
            CreatedAt = pod.CreatedAt,
            UpdatedAt = pod.UpdatedAt,
            LastStartedAt = pod.LastStartedAt,
            LastStoppedAt = pod.LastStoppedAt,
            LastSyncedAt = pod.LastSyncedAt,
            Endpoints = pod.Endpoints
                .Select(e => new PodEndpointResponse
                {
                    Port = e.Port,
                    Protocol = e.Protocol,
                    PublicPort = e.PublicPort,
                    Url = e.Url,
                })
                .ToList(),
            StatusHistory = history,
            Configuration = pod.Configuration is null
                ? null
                : new PodConfigurationResponse
                {
                    TemplateId = pod.Configuration.TemplateId,
                    TemplateName = pod.Configuration.TemplateName,
                    ImageName = pod.Configuration.ImageName,
                    ContainerDiskGb = pod.Configuration.ContainerDiskGb,
                    VolumeDiskGb = pod.Configuration.VolumeDiskGb,
                    VolumeMountPath = pod.Configuration.VolumeMountPath,
                    GpuCount = pod.Configuration.GpuCount,
                    EnvironmentVariables = PodAccess.DeserializeEnvironmentVariables(
                        pod.Configuration.EnvironmentVariablesJson),
                    Ports = PodAccess.DeserializePorts(pod.Configuration.PortsJson),
                    EnablePublicIp = pod.Configuration.EnablePublicIp,
                },
        };
    }

    private static string FormatStatus(PodStatus status) =>
        status switch
        {
            PodStatus.Creating => nameof(PodStatus.BuildingPending),
            _ => status.ToString(),
        };
}
