namespace PodPilot.Domain.Entities;

/// <summary>
/// Associates a model with a pod pool.
/// </summary>
public class PodPoolModel : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the pod pool identifier.
    /// </summary>
    public Guid PodPoolId { get; set; }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the pod pool.
    /// </summary>
    public PodPool PodPool { get; set; } = null!;
}
