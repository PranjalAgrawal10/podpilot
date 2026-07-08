using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Resolves request priority from auth context and request metadata.
/// </summary>
public interface IRequestPriorityResolver
{
    /// <summary>
    /// Resolves the priority for a gateway request.
    /// </summary>
    RequestPriority Resolve(GatewayAuthContext auth, string path, bool isStreaming);
}
