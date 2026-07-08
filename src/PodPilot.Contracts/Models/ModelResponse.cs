namespace PodPilot.Contracts.Models;

/// <summary>
/// AI model summary response.
/// </summary>
public class ModelResponse
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the pod identifier.
    /// </summary>
    public Guid PodId { get; set; }

    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string PodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model tag.
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full model reference.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

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
    /// Gets or sets the parameter label.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the license.
    /// </summary>
    public string? License { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default model.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the model status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the model was last used.
    /// </summary>
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// Gets or sets when the model was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the model was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
