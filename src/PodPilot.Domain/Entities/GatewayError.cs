using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Error details for a failed gateway request.
/// </summary>
public class GatewayError
{
    /// <summary>
    /// Gets or sets the gateway request identifier.
    /// </summary>
    public Guid GatewayRequestId { get; set; }

    /// <summary>
    /// Gets or sets the error format.
    /// </summary>
    public GatewayErrorFormat ErrorFormat { get; set; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets internal details (not exposed to clients).
    /// </summary>
    public string? InternalDetails { get; set; }

    /// <summary>
    /// Gets or sets when the error occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Gets the gateway request.
    /// </summary>
    public GatewayRequest Request { get; set; } = null!;
}
