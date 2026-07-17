using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Deployments.Cloud;

/// <summary>
/// RunPod cloud adapter for one-click deployments.
/// </summary>
public sealed class RunPodDeploymentCloudAdapter : IDeploymentCloudAdapter
{
    /// <inheritdoc />
    public DeploymentCloudProviderKind Kind => DeploymentCloudProviderKind.RunPod;

    /// <inheritdoc />
    public bool IsImplemented => true;

    /// <inheritdoc />
    public ProviderType ToProviderType() => ProviderType.RunPod;
}

/// <summary>
/// Vast.ai stub adapter.
/// </summary>
public sealed class VastAiDeploymentCloudAdapter : IDeploymentCloudAdapter
{
    /// <inheritdoc />
    public DeploymentCloudProviderKind Kind => DeploymentCloudProviderKind.VastAi;

    /// <inheritdoc />
    public bool IsImplemented => false;

    /// <inheritdoc />
    public ProviderType ToProviderType() => ProviderType.Vast;
}

/// <summary>
/// Lambda Labs stub adapter.
/// </summary>
public sealed class LambdaLabsDeploymentCloudAdapter : IDeploymentCloudAdapter
{
    /// <inheritdoc />
    public DeploymentCloudProviderKind Kind => DeploymentCloudProviderKind.LambdaLabs;

    /// <inheritdoc />
    public bool IsImplemented => false;

    /// <inheritdoc />
    public ProviderType ToProviderType() => ProviderType.Lambda;
}

/// <summary>
/// Azure GPU stub adapter.
/// </summary>
public sealed class AzureGpuDeploymentCloudAdapter : IDeploymentCloudAdapter
{
    /// <inheritdoc />
    public DeploymentCloudProviderKind Kind => DeploymentCloudProviderKind.AzureGpu;

    /// <inheritdoc />
    public bool IsImplemented => false;

    /// <inheritdoc />
    public ProviderType ToProviderType() => ProviderType.Azure;
}

/// <summary>
/// AWS GPU stub adapter.
/// </summary>
public sealed class AwsGpuDeploymentCloudAdapter : IDeploymentCloudAdapter
{
    /// <inheritdoc />
    public DeploymentCloudProviderKind Kind => DeploymentCloudProviderKind.AwsGpu;

    /// <inheritdoc />
    public bool IsImplemented => false;

    /// <inheritdoc />
    public ProviderType ToProviderType() => ProviderType.AWS;
}

/// <summary>
/// Google Cloud GPU stub adapter.
/// </summary>
public sealed class GoogleCloudGpuDeploymentCloudAdapter : IDeploymentCloudAdapter
{
    /// <inheritdoc />
    public DeploymentCloudProviderKind Kind => DeploymentCloudProviderKind.GoogleCloudGpu;

    /// <inheritdoc />
    public bool IsImplemented => false;

    /// <inheritdoc />
    public ProviderType ToProviderType() => ProviderType.GoogleCloud;
}

/// <summary>
/// Kubernetes GPU stub adapter.
/// </summary>
public sealed class KubernetesDeploymentCloudAdapter : IDeploymentCloudAdapter
{
    /// <inheritdoc />
    public DeploymentCloudProviderKind Kind => DeploymentCloudProviderKind.Kubernetes;

    /// <inheritdoc />
    public bool IsImplemented => false;

    /// <inheritdoc />
    public ProviderType ToProviderType() => ProviderType.Kubernetes;
}

/// <summary>
/// Resolves deployment cloud adapters.
/// </summary>
public sealed class DeploymentCloudAdapterFactory : IDeploymentCloudAdapterFactory
{
    private readonly IReadOnlyDictionary<DeploymentCloudProviderKind, IDeploymentCloudAdapter> adapters;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentCloudAdapterFactory"/> class.
    /// </summary>
    public DeploymentCloudAdapterFactory(IEnumerable<IDeploymentCloudAdapter> adapters)
    {
        this.adapters = adapters.ToDictionary(a => a.Kind);
    }

    /// <inheritdoc />
    public IDeploymentCloudAdapter GetAdapter(DeploymentCloudProviderKind kind)
    {
        if (adapters.TryGetValue(kind, out var adapter))
        {
            return adapter;
        }

        throw new InvalidOperationException($"No cloud adapter registered for {kind}.");
    }

    /// <summary>
    /// Gets an adapter for a compute <see cref="ProviderType"/>.
    /// </summary>
    public IDeploymentCloudAdapter GetAdapterForProviderType(ProviderType providerType)
    {
        var adapter = adapters.Values.FirstOrDefault(a => a.ToProviderType() == providerType);
        if (adapter is null)
        {
            throw new InvalidOperationException($"No cloud adapter maps to provider type {providerType}.");
        }

        if (!adapter.IsImplemented)
        {
            throw new InvalidOperationException(
                $"Cloud provider {adapter.Kind} is not yet supported for one-click deployments.");
        }

        return adapter;
    }
}
