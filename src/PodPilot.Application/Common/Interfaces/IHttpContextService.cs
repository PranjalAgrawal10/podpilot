namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Provides HTTP context information for the current request.
/// </summary>
public interface IHttpContextService
{
    /// <summary>
    /// Gets the client IP address.
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the request correlation identifier.
    /// </summary>
    string? CorrelationId { get; }
}
