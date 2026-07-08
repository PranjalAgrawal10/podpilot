namespace PodPilot.Domain.Enums;

/// <summary>
/// Lifecycle status of an AI model on a GPU pod.
/// </summary>
public enum ModelStatus
{
    /// <summary>Model is being downloaded from Ollama.</summary>
    Downloading = 0,

    /// <summary>Model is installed and ready to use.</summary>
    Available = 1,

    /// <summary>Model is being loaded into memory.</summary>
    Loading = 2,

    /// <summary>Model is actively running inference.</summary>
    Running = 3,

    /// <summary>Model operation failed.</summary>
    Failed = 4,

    /// <summary>Model was removed from the pod.</summary>
    Deleted = 5,
}
