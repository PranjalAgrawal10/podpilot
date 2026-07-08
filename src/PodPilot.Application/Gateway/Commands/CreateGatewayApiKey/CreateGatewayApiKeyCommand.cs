using MediatR;
using PodPilot.Contracts.Gateway;

namespace PodPilot.Application.Gateway.Commands.CreateGatewayApiKey;

/// <summary>
/// Creates a gateway API key.
/// </summary>
public sealed class CreateGatewayApiKeyCommand : IRequest<GatewayApiKeyResponse>
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a personal key.
    /// </summary>
    public bool IsPersonal { get; init; }

    /// <summary>
    /// Gets or sets optional expiration timestamp.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Gets or sets requests allowed per minute.
    /// </summary>
    public int? RateLimitPerMinute { get; init; }

    /// <summary>
    /// Gets or sets requests allowed per day.
    /// </summary>
    public int? RateLimitPerDay { get; init; }
}
