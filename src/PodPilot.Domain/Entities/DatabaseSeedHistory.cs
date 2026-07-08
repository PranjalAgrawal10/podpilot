namespace PodPilot.Domain.Entities;

/// <summary>
/// Audit record of a database seeder execution.
/// </summary>
public class DatabaseSeedHistory : Common.BaseEntity
{
    /// <summary>
    /// Gets or sets the seeder name.
    /// </summary>
    public string SeederName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the seeder version or checksum.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the seeder ran.
    /// </summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the seeder completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets optional details about the seed operation.
    /// </summary>
    public string? Details { get; set; }
}
