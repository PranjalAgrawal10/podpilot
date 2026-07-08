using MediatR;
using PodPilot.Contracts.Pods;

namespace PodPilot.Application.Pods.Queries.ListPods;

/// <summary>
/// Query to list organization pods.
/// </summary>
public sealed class ListPodsQuery : IRequest<IReadOnlyList<PodResponse>>
{
}
