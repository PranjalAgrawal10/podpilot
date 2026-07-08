namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Gateway API key authentication constants.
/// </summary>
public static class GatewayAuthConstants
{
    /// <summary>
    /// Authentication scheme name.
    /// </summary>
    public const string SchemeName = "GatewayApiKey";

    /// <summary>
    /// HttpContext item key for the resolved auth context.
    /// </summary>
    public const string AuthContextItemKey = "GatewayAuthContext";
}
