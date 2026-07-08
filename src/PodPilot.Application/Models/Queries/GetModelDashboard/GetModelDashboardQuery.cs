using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Queries.GetModelDashboard;

/// <summary>
/// Gets model management dashboard metrics.
/// </summary>
public sealed class GetModelDashboardQuery : IRequest<ModelDashboardResponse>
{
    /// <summary>
    /// Gets or sets an optional pod filter.
    /// </summary>
    public Guid? PodId { get; set; }
}
