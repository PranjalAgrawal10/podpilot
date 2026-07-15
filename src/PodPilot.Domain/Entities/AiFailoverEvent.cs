namespace PodPilot.Domain.Entities;

/// <summary>
/// Record of an AI provider failover event.
/// </summary>
public class AiFailoverEvent : Common.BaseEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets when the failover occurred.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Gets or sets the requested model name.</summary>
    public string? ModelName { get; set; }

    /// <summary>Gets or sets the failed provider identifier.</summary>
    public Guid FromProviderId { get; set; }

    /// <summary>Gets or sets the provider that handled the request after failover.</summary>
    public Guid? ToProviderId { get; set; }

    /// <summary>Gets or sets the failure reason.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the related gateway request identifier when available.</summary>
    public Guid? GatewayRequestId { get; set; }

    /// <summary>Gets or sets a value indicating whether failover succeeded.</summary>
    public bool Succeeded { get; set; }
}
