using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.GetOrchestratorStatus;

/// <summary>
/// Gets orchestrator status for the current organization.
/// </summary>
public sealed class GetOrchestratorStatusQuery : IRequest<OrchestratorStatusResponse>
{
}
