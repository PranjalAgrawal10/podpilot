using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents an Ollama model installed on a GPU pod.
/// </summary>
public class AiModel : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the owning organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the GPU pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the model base name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model tag.
    /// </summary>
    public string Tag { get; set; } = "latest";

    /// <summary>
    /// Gets or sets the model family.
    /// </summary>
    public string? Family { get; set; }

    /// <summary>
    /// Gets or sets the model size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the quantization format.
    /// </summary>
    public string? Quantization { get; set; }

    /// <summary>
    /// Gets or sets the context window length.
    /// </summary>
    public int? ContextLength { get; set; }

    /// <summary>
    /// Gets or sets the parameter count label (e.g. 7B).
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the model license.
    /// </summary>
    public string? License { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the default model for the pod.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the current model status.
    /// </summary>
    public ModelStatus Status { get; set; } = ModelStatus.Available;

    /// <summary>
    /// Gets or sets when the model was last used for inference.
    /// </summary>
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Gets the GPU pod.
    /// </summary>
    public GpuPod Pod { get; set; } = null!;

    /// <summary>
    /// Gets download history for this model.
    /// </summary>
    public ICollection<ModelDownload> Downloads { get; set; } = [];

    /// <summary>
    /// Gets health check history for this model.
    /// </summary>
    public ICollection<ModelHealthHistory> HealthHistory { get; set; } = [];

    /// <summary>
    /// Gets the full Ollama model reference.
    /// </summary>
    public string FullName =>
        Tag == "latest" || Tag == string.Empty ? Name : $"{Name}:{Tag}";
}
