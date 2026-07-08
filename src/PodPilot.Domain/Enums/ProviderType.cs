namespace PodPilot.Domain.Enums;

/// <summary>
/// Supported compute provider types.
/// </summary>
public enum ProviderType
{
    /// <summary>RunPod GPU cloud.</summary>
    RunPod = 0,

    /// <summary>Vast.ai marketplace.</summary>
    Vast = 1,

    /// <summary>Lambda Labs.</summary>
    Lambda = 2,

    /// <summary>Microsoft Azure.</summary>
    Azure = 3,

    /// <summary>Amazon Web Services.</summary>
    AWS = 4,

    /// <summary>Google Cloud Platform.</summary>
    GoogleCloud = 5,

    /// <summary>Self-managed Kubernetes cluster.</summary>
    Kubernetes = 6,
}
