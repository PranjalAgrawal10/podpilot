namespace PodPilot.Domain.Entities;

/// <summary>
/// Key/value setting for a plugin installation. Secrets are stored encrypted.
/// </summary>
public class PluginSetting : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the plugin installation identifier.</summary>
    public Guid PluginInstallationId { get; set; }

    /// <summary>Gets or sets the setting key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the setting value (plaintext or ciphertext).</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the value is a secret.</summary>
    public bool IsSecret { get; set; }

    /// <summary>Gets the plugin installation.</summary>
    public PluginInstallation PluginInstallation { get; set; } = null!;
}
