namespace PodPilot.Contracts.Plugins;

/// <summary>Plugin catalog / installation response.</summary>
public sealed class PluginResponse
{
    public Guid Id { get; init; }
    public Guid? InstallationId { get; init; }
    public string PackageId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string PluginType { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Publisher { get; init; } = string.Empty;
    public bool IsFirstParty { get; init; }
    public string? Status { get; init; }
    public bool? IsHealthy { get; init; }
    public string? HealthMessage { get; init; }
    public IReadOnlyList<string> RequiredPermissions { get; init; } = [];
    public IReadOnlyList<string> GrantedPermissions { get; init; } = [];
    public DateTime? EnabledAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Install plugin request.</summary>
public sealed class InstallPluginRequest
{
    public string PackageId { get; set; } = string.Empty;
    public IReadOnlyList<string>? GrantedPermissions { get; set; }
}

/// <summary>Update plugin installation request.</summary>
public sealed class UpdatePluginRequest
{
    public IReadOnlyList<string>? GrantedPermissions { get; set; }
}

/// <summary>Plugin setting item (secrets never returned).</summary>
public sealed class PluginSettingResponse
{
    public string Key { get; init; } = string.Empty;
    public string? Value { get; init; }
    public bool IsSecret { get; init; }
    public bool HasValue { get; init; }
}

/// <summary>Update settings request.</summary>
public sealed class UpdatePluginSettingsRequest
{
    public Dictionary<string, string> Settings { get; set; } = [];
    public IReadOnlyList<string>? SecretKeys { get; set; }
}

/// <summary>Plugin dashboard response.</summary>
public sealed class PluginDashboardResponse
{
    public int InstalledPlugins { get; init; }
    public int EnabledPlugins { get; init; }
    public int ConnectedMcpServers { get; init; }
    public int AvailableTools { get; init; }
    public int UnhealthyPlugins { get; init; }
    public int RecentExecutions { get; init; }
}
