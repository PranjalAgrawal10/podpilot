using MediatR;
using PodPilot.Contracts.Observability;

namespace PodPilot.Application.Observability.Queries.GetSystemHealth;

/// <summary>
/// Gets system health for the current organization.
/// </summary>
public sealed class GetSystemHealthQuery : IRequest<SystemHealthResponse>
{
}
