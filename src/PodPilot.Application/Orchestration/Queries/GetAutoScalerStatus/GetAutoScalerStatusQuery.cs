using MediatR;
using PodPilot.Contracts.Orchestration;

namespace PodPilot.Application.Orchestration.Queries.GetAutoScalerStatus;

/// <summary>
/// Gets auto-scaler status for the current organization.
/// </summary>
public sealed class GetAutoScalerStatusQuery : IRequest<AutoScalerStatusResponse>
{
}
