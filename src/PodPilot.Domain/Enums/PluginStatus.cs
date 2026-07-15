namespace PodPilot.Domain.Enums;

/// <summary>
/// Lifecycle status of a plugin installation.
/// </summary>
public enum PluginStatus
{
    /// <summary>Installed but not enabled.</summary>
    Installed = 0,

    /// <summary>Enabled and available.</summary>
    Enabled = 1,

    /// <summary>Disabled by an administrator.</summary>
    Disabled = 2,

    /// <summary>Failed to load or initialize.</summary>
    Failed = 3,

    /// <summary>Marked for removal.</summary>
    Uninstalling = 4,
}
