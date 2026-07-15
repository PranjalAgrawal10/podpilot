namespace PodPilot.Domain.Entities;

/// <summary>
/// Audit/diagnostic log entry for plugin lifecycle and execution.
/// </summary>
public class PluginLog : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the optional plugin installation identifier.</summary>
    public Guid? PluginInstallationId { get; set; }

    /// <summary>Gets or sets the log level (Information, Warning, Error).</summary>
    public string Level { get; set; } = "Information";

    /// <summary>Gets or sets the event category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets optional structured details as JSON.</summary>
    public string? DetailsJson { get; set; }

    /// <summary>Gets or sets when the event occurred.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Gets the plugin installation.</summary>
    public PluginInstallation? PluginInstallation { get; set; }
}
