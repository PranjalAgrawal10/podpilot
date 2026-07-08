namespace PodPilot.Domain.Enums;

/// <summary>
/// Type of gateway API key.
/// </summary>
public enum GatewayApiKeyType
{
    /// <summary>Personal key scoped to a user.</summary>
    Personal = 0,

    /// <summary>Organization-wide key.</summary>
    Organization = 1,
}
