namespace PodPilot.Domain.Entities;

/// <summary>
/// Audit record of an applied EF Core database migration.
/// </summary>
public class DatabaseMigrationHistory : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the EF migration identifier.
    /// </summary>
    public string MigrationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the EF Core product version that applied the migration.
    /// </summary>
    public string ProductVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the migration was applied.
    /// </summary>
    public DateTime AppliedAt { get; set; }
}
