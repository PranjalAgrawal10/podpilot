using PodPilot.Application.Common;
using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Entities;

namespace PodPilot.Application.Pods;

/// <summary>
/// Builds provider create options from persisted pod configuration.
/// </summary>
public static class PodCreateOptionsFactory
{
    /// <summary>
    /// Creates provider options from a pod and its stored configuration.
    /// </summary>
    public static PodCreateOptions FromConfiguration(GpuPod pod, PodConfiguration configuration) =>
        new()
        {
            Name = BuildReplacementProviderName(pod),
            GpuId = pod.GpuId,
            GpuType = pod.GpuType,
            Region = pod.Region,
            TemplateId = configuration.TemplateId,
            TemplateName = configuration.TemplateName,
            ImageName = configuration.ImageName,
            ContainerDiskGb = configuration.ContainerDiskGb,
            VolumeDiskGb = configuration.VolumeDiskGb,
            VolumeMountPath = configuration.VolumeMountPath,
            GpuCount = configuration.GpuCount,
            EnvironmentVariables = PodAccess.DeserializeEnvironmentVariables(configuration.EnvironmentVariablesJson),
            Ports = PodAccess.DeserializePorts(configuration.PortsJson),
            EnablePublicIp = configuration.EnablePublicIp,
        };

    /// <summary>
    /// Builds a unique provider-side name for a replacement pod.
    /// </summary>
    public static string BuildReplacementProviderName(GpuPod pod)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var candidate = $"{pod.Name}-recovery-{suffix}";
        return candidate.Length <= ApplicationConstants.PodNameMaxLength
            ? candidate
            : candidate[..ApplicationConstants.PodNameMaxLength];
    }
}
