namespace PodPilot.Domain.Enums;

/// <summary>
/// Error response format for gateway clients.
/// </summary>
public enum GatewayErrorFormat
{
    /// <summary>OpenAI-compatible error envelope.</summary>
    OpenAi = 0,

    /// <summary>Anthropic-compatible error envelope.</summary>
    Anthropic = 1,
}
