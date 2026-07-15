using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Catalog definition for a first-party or third-party plugin package.
/// </summary>
public class PluginDefinition : Common.AuditableEntity
{
    /// <summary>Gets or sets the unique plugin package identifier (e.g. com.podpilot.notifications.slack).</summary>
    public string PackageId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the semantic version.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the plugin type.</summary>
    public PluginType PluginType { get; set; }

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the publisher name.</summary>
    public string Publisher { get; set; } = "PodPilot";

    /// <summary>Gets or sets whether this is a first-party plugin.</summary>
    public bool IsFirstParty { get; set; }

    /// <summary>Gets or sets the entry assembly name for hot-loading.</summary>
    public string? EntryAssembly { get; set; }

    /// <summary>Gets or sets the entry type full name.</summary>
    public string? EntryType { get; set; }

    /// <summary>Gets or sets required permissions as JSON array of strings.</summary>
    public string RequiredPermissionsJson { get; set; } = "[]";

    /// <summary>Gets or sets default settings schema as JSON.</summary>
    public string? SettingsSchemaJson { get; set; }

    /// <summary>Gets or sets a value indicating whether the package is publicly listable in the local marketplace.</summary>
    public bool IsListed { get; set; } = true;

    /// <summary>Gets installations of this plugin.</summary>
    public ICollection<PluginInstallation> Installations { get; set; } = [];
}
