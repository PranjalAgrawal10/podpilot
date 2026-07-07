namespace PodPilot.Domain.Common;

/// <summary>
/// Base entity with a strongly-typed identifier.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
}
