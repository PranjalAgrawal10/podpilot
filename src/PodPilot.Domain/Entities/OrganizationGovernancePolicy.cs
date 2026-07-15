namespace PodPilot.Domain.Entities;

/// <summary>
/// Organization governance limits and allow-lists (providers, models, cost, plugins, MCP).
/// </summary>
public class OrganizationGovernancePolicy : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets allowed compute/AI provider kinds as JSON array.</summary>
    public string AllowedProvidersJson { get; set; } = "[]";

    /// <summary>Gets or sets allowed model name patterns as JSON array.</summary>
    public string AllowedModelsJson { get; set; } = "[]";

    /// <summary>Gets or sets maximum GPU hourly cost USD (null = unlimited).</summary>
    public decimal? MaximumGpuCostPerHour { get; set; }

    /// <summary>Gets or sets maximum concurrently running pods.</summary>
    public int? MaximumRunningPods { get; set; }

    /// <summary>Gets or sets maximum scheduler queue size.</summary>
    public int? MaximumQueueSize { get; set; }

    /// <summary>Gets or sets maximum daily spend USD.</summary>
    public decimal? MaximumDailySpendUsd { get; set; }

    /// <summary>Gets or sets allowed plugin package ids as JSON array.</summary>
    public string AllowedPluginsJson { get; set; } = "[]";

    /// <summary>Gets or sets allowed MCP server kinds as JSON array.</summary>
    public string AllowedMcpServersJson { get; set; } = "[]";

    /// <summary>Gets or sets whether empty allow-lists mean allow-all.</summary>
    public bool EmptyAllowListMeansAllowAll { get; set; } = true;

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;
}
