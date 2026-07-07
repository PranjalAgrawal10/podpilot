namespace PodPilot.Domain.Common;

/// <summary>
/// Base entity with audit metadata.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public string? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <inheritdoc />
    public string? UpdatedBy { get; set; }
}
