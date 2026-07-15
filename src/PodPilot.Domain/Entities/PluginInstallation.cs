using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Organization-scoped installation of a plugin.
/// </summary>
public class PluginInstallation : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the plugin definition identifier.</summary>
    public Guid PluginDefinitionId { get; set; }

    /// <summary>Gets or sets the installation status.</summary>
    public PluginStatus Status { get; set; } = PluginStatus.Installed;

    /// <summary>Gets or sets when the plugin was enabled.</summary>
    public DateTime? EnabledAt { get; set; }

    /// <summary>Gets or sets when the plugin was last health-checked.</summary>
    public DateTime? LastHealthCheckAt { get; set; }

    /// <summary>Gets or sets the last health message.</summary>
    public string? HealthMessage { get; set; }

    /// <summary>Gets or sets a value indicating whether the latest health check succeeded.</summary>
    public bool IsHealthy { get; set; } = true;

    /// <summary>Gets or sets granted permissions as JSON array of strings.</summary>
    public string GrantedPermissionsJson { get; set; } = "[]";

    /// <summary>Gets the plugin definition.</summary>
    public PluginDefinition PluginDefinition { get; set; } = null!;

    /// <summary>Gets settings for this installation.</summary>
    public ICollection<PluginSetting> Settings { get; set; } = [];

    /// <summary>Gets logs for this installation.</summary>
    public ICollection<PluginLog> Logs { get; set; } = [];
}
