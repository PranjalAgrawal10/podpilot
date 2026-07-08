using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Scheduler;

/// <summary>
/// Resolves request priority from auth context and request metadata.
/// </summary>
public sealed class RequestPriorityResolver : IRequestPriorityResolver
{
    /// <inheritdoc />
    public RequestPriority Resolve(GatewayAuthContext auth, string path, bool isStreaming)
    {
        if (isStreaming)
        {
            return RequestPriority.High;
        }

        if (auth.UserId.HasValue)
        {
            return RequestPriority.High;
        }

        if (path.Contains("/batch", StringComparison.OrdinalIgnoreCase))
        {
            return RequestPriority.Low;
        }

        if (path.Contains("/background", StringComparison.OrdinalIgnoreCase))
        {
            return RequestPriority.Background;
        }

        return RequestPriority.Normal;
    }
}
