namespace PodPilot.Domain.Common;

/// <summary>
/// Defines audit metadata for entities.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was last updated.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; set; }
}
